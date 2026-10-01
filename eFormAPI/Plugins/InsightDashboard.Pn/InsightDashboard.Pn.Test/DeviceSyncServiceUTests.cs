/*
The MIT License (MIT)

Copyright (c) 2007 - 2021 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace InsightDashboard.Pn.Test;

using System;
using System.Linq;
using System.Threading.Tasks;
using Base;
using Helpers;
using Infrastructure.Models.Device;
using Infrastructure.Models.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microting.eForm.Infrastructure.Data.Entities;
using NSubstitute;
using NUnit.Framework;
using Services.DeviceSyncService;
using Services.OptionsService;
using Services.QuestionsService;
using Services.QuestionSetsService;

// Denne service er selve tablet-vendte API'et (parring, hentning, upload) —
// den del insight_app rent faktisk taler med. Parring sker via en
// tidsbegrænset engangskode (DevicePairingCodeStore): PairViaNewCode()
// efterligner det en admin ville gøre (anmode om en kode) efterfulgt af det
// enheden ville gøre (indløse koden).
[TestFixture]
public class DeviceSyncServiceUTests : DbTestFixture
{
    private const int TestSiteMicrotingUid = 9990001;

    private QuestionSetsService _questionSetsService;
    private QuestionsService _questionsService;
    private OptionsService _optionsService;
    private DeviceSyncService _deviceSyncService;

    private Site _site;
    private SurveyConfiguration _surveyConfiguration;
    private SiteSurveyConfiguration _siteSurveyConfiguration;
    private int _setId;

    protected override void DoSetup()
    {
        // Se QuestionSetsServiceUTests for hvorfor dette skal ske først.
        Microting.eForm.Infrastructure.Data.Entities.Language.AddDefaultLanguages(DbContext).GetAwaiter().GetResult();

        var coreHelper = InsightCoreTestHelper.BuildFakeCoreService(ConnectionString);
        var localizationService = MockHelper.GetLocalizationService();

        _questionSetsService = new QuestionSetsService(
            Substitute.For<ILogger<QuestionSetsService>>(), localizationService, coreHelper);
        _questionsService = new QuestionsService(
            Substitute.For<ILogger<QuestionsService>>(), localizationService, coreHelper);
        _optionsService = new OptionsService(
            Substitute.For<ILogger<OptionsService>>(), localizationService, coreHelper);
        _deviceSyncService = new DeviceSyncService(
            Substitute.For<ILogger<DeviceSyncService>>(), localizationService, coreHelper,
            new DeviceTokenStore(), new DevicePairingCodeStore());

        // Ét spørgeskema med ét buttons-spørgsmål og to svarmuligheder,
        // koblet til et testsite via en survey configuration — nøjagtig den
        // kæde GetQuestionSetForToken slår op igennem.
        _setId = _questionSetsService.Create(new QuestionSetCreateModel { Name = "UTest device set" })
            .GetAwaiter().GetResult().Model;
        var questionId = _questionsService.Create(new QuestionCreateModel
            { QuestionSetId = _setId, QuestionType = "buttons", Text = "Hvordan var det?" })
            .GetAwaiter().GetResult().Model;
        _optionsService.Create(new OptionCreateModel { QuestionId = questionId, Label = "Godt" })
            .GetAwaiter().GetResult();

        _site = new Site { Name = "UTest site", MicrotingUid = TestSiteMicrotingUid };
        _site.Create(DbContext).GetAwaiter().GetResult();

        _surveyConfiguration = new SurveyConfiguration
        {
            Name = "UTest configuration",
            QuestionSetId = _setId,
            Start = DateTime.UtcNow,
            Stop = DateTime.UtcNow.AddYears(1),
        };
        _surveyConfiguration.Create(DbContext).GetAwaiter().GetResult();

        _siteSurveyConfiguration = new SiteSurveyConfiguration
        {
            SiteId = _site.Id,
            SurveyConfigurationId = _surveyConfiguration.Id,
        };
        _siteSurveyConfiguration.Create(DbContext).GetAwaiter().GetResult();
    }

    [TearDown]
    public void CleanupDeviceFixtures()
    {
        _siteSurveyConfiguration.Delete(DbContext).GetAwaiter().GetResult();
        _surveyConfiguration.Delete(DbContext).GetAwaiter().GetResult();
        _site.Delete(DbContext).GetAwaiter().GetResult();
        _questionSetsService.Delete(_setId).GetAwaiter().GetResult();
    }

    // Efterligner det fulde parringsflow: en admin anmoder om en kode for
    // sitet (RequestPairingCode, som tager Sites.Id — den interne PK, samme
    // id som /api/sites/dictionary bruger), og enheden indløser den med det
    // samme (Pair) — nøjagtig som insight_app ville gøre efter en admin har
    // tastet/scannet koden.
    private async Task<string> PairViaNewCode()
    {
        var codeResult = await _deviceSyncService.RequestPairingCode(_site.Id);
        var pairResult = await _deviceSyncService.Pair(codeResult.Model.Code);
        return pairResult.Model.Token;
    }

    [Test]
    public async Task RequestPairingCode_KnownSite_ReturnsSixDigitCode()
    {
        var result = await _deviceSyncService.RequestPairingCode(_site.Id);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Model.Code, Has.Length.EqualTo(6));
        Assert.That(result.Model.Code, Does.Match("^[0-9]{6}$"));
        Assert.That(result.Model.SiteId, Is.EqualTo(_site.Id));
        Assert.That(result.Model.SiteName, Is.EqualTo("UTest site"));
        Assert.That(result.Model.ExpiresAtUtc, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public async Task RequestPairingCode_UnknownSite_ReturnsFailure()
    {
        var result = await _deviceSyncService.RequestPairingCode(-1);

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task Pair_ValidCode_ReturnsTokenAndSiteInfo()
    {
        var codeResult = await _deviceSyncService.RequestPairingCode(_site.Id);

        var result = await _deviceSyncService.Pair(codeResult.Model.Code);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Model.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Model.SiteId, Is.EqualTo(_site.Id));
        Assert.That(result.Model.SiteName, Is.EqualTo("UTest site"));
    }

    [Test]
    public async Task Pair_UnknownCode_ReturnsFailure()
    {
        var result = await _deviceSyncService.Pair("000000");

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task Pair_CodeIsSingleUse_SecondAttemptWithSameCodeFails()
    {
        var codeResult = await _deviceSyncService.RequestPairingCode(_site.Id);
        var firstAttempt = await _deviceSyncService.Pair(codeResult.Model.Code);
        var secondAttempt = await _deviceSyncService.Pair(codeResult.Model.Code);

        Assert.That(firstAttempt.Success, Is.True);
        Assert.That(secondAttempt.Success, Is.False);
    }

    [Test]
    public async Task RequestPairingCode_Regenerating_InvalidatesPreviousCode()
    {
        var firstCode = (await _deviceSyncService.RequestPairingCode(_site.Id)).Model.Code;
        await _deviceSyncService.RequestPairingCode(_site.Id);

        var result = await _deviceSyncService.Pair(firstCode);

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task GetQuestionSetForToken_UnpairedToken_ReturnsFailure()
    {
        var result = await _deviceSyncService.GetQuestionSetForToken("not-a-real-token");

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task GetQuestionSetForToken_ReturnsAssignedQuestionSetWithQuestionsAndOptions()
    {
        var token = await PairViaNewCode();

        var result = await _deviceSyncService.GetQuestionSetForToken(token);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Model.Id, Is.EqualTo(_setId));
        Assert.That(result.Model.SurveyConfigurationId, Is.EqualTo(_surveyConfiguration.Id));
        Assert.That(result.Model.Questions, Has.Count.EqualTo(1));

        var question = result.Model.Questions.Single();
        Assert.That(question.QuestionType, Is.EqualTo("buttons"));
        Assert.That(question.Text, Is.EqualTo("Hvordan var det?"));
        Assert.That(question.Options.Select(o => o.Label), Is.EqualTo(new[] { "Godt" }));
    }

    [Test]
    public async Task SubmitAnswerCycle_WritesAnswerAndAnswerValues()
    {
        var token = await PairViaNewCode();
        var questionSet = (await _deviceSyncService.GetQuestionSetForToken(token)).Model;
        var question = questionSet.Questions.Single();
        var option = question.Options.Single();

        var cycle = new DeviceAnswerCycleModel
        {
            UnitAnswerId = "utest-cycle-1",
            QuestionSetId = questionSet.Id.ToString(),
            SurveyConfigurationId = questionSet.SurveyConfigurationId,
            LanguageId = 1,
            FinishedAt = DateTime.UtcNow.ToString("O"),
            Answers =
            {
                new DeviceAnswerItemModel
                {
                    QuestionId = question.Id.ToString(),
                    AnswerValues = { option.Id.ToString() },
                    QuestionDuration = 4200,
                },
            },
        };

        var submitResult = await _deviceSyncService.SubmitAnswerCycle(token, cycle);

        Assert.That(submitResult.Success, Is.True);
        Assert.That(submitResult.Model.UnitAnswerId, Is.EqualTo("utest-cycle-1"));

        var answer = await DbContext.Answers
            .Where(x => x.SiteId == _site.Id)
            .Where(x => x.QuestionSetId == questionSet.Id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        Assert.That(answer, Is.Not.Null);
        Assert.That(answer!.AnswerDuration, Is.EqualTo(4)); // 4200 ms rundet til hele sekunder

        var answerValue = await DbContext.AnswerValues
            .Where(x => x.AnswerId == answer.Id)
            .FirstOrDefaultAsync();

        Assert.That(answerValue, Is.Not.Null);
        Assert.That(answerValue!.QuestionId, Is.EqualTo(question.Id));
        Assert.That(answerValue.OptionId, Is.EqualTo(option.Id));

        // Selvstændig oprydning: disse to rammes ikke af CleanupDeviceFixtures.
        await answerValue.Delete(DbContext);
        await answer.Delete(DbContext);
    }
}
