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

using System.Linq;
using System.Threading.Tasks;
using Base;
using Helpers;
using Infrastructure.Models.Questions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Services.OptionsService;
using Services.QuestionsService;
using Services.QuestionSetsService;

// Kun buttons/list/multi kræver manuel options-håndtering (se
// OptionsService's egen kommentar) — derfor bruges "buttons" som
// spørgsmålstype i alle tests her.
[TestFixture]
public class OptionsServiceUTests : DbTestFixture
{
    private QuestionSetsService _questionSetsService;
    private QuestionsService _questionsService;
    private OptionsService _optionsService;
    private int _setId;
    private int _questionId;

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

        _setId = _questionSetsService.Create(new QuestionSetCreateModel { Name = "UTest options set" })
            .GetAwaiter().GetResult().Model;
        _questionId = _questionsService.Create(new QuestionCreateModel
            { QuestionSetId = _setId, QuestionType = "buttons", Text = "Vælg en mulighed" })
            .GetAwaiter().GetResult().Model;
    }

    [TearDown]
    public void CleanupQuestionSet()
    {
        _questionSetsService.Delete(_setId).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Create_AssignsSequentialOptionIndex()
    {
        var firstId = (await _optionsService.Create(new OptionCreateModel { QuestionId = _questionId, Label = "Godt" })).Model;
        var secondId = (await _optionsService.Create(new OptionCreateModel { QuestionId = _questionId, Label = "Dårligt" })).Model;

        var set = (await _questionSetsService.Get(_setId)).Model;
        var options = set.Questions.Single(x => x.Id == _questionId).Options;

        Assert.That(options.Single(x => x.Id == firstId).OptionIndex, Is.EqualTo(0));
        Assert.That(options.Single(x => x.Id == secondId).OptionIndex, Is.EqualTo(1));
    }

    [Test]
    public async Task Create_UnknownQuestion_ReturnsFailure()
    {
        var result = await _optionsService.Create(new OptionCreateModel { QuestionId = int.MaxValue, Label = "x" });

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task Update_ChangesLabel()
    {
        var optionId = (await _optionsService.Create(new OptionCreateModel { QuestionId = _questionId, Label = "Før" })).Model;

        var updateResult = await _optionsService.Update(new OptionUpdateModel { Id = optionId, Label = "Efter" });
        Assert.That(updateResult.Success, Is.True);

        var set = (await _questionSetsService.Get(_setId)).Model;
        var option = set.Questions.Single(x => x.Id == _questionId).Options.Single(x => x.Id == optionId);

        Assert.That(option.Label, Is.EqualTo("Efter"));
    }

    [Test]
    public async Task Delete_RemovesOption()
    {
        var optionId = (await _optionsService.Create(new OptionCreateModel { QuestionId = _questionId, Label = "Skal slettes" })).Model;

        var deleteResult = await _optionsService.Delete(optionId);
        Assert.That(deleteResult.Success, Is.True);

        var set = (await _questionSetsService.Get(_setId)).Model;
        var options = set.Questions.Single(x => x.Id == _questionId).Options;

        Assert.That(options.Any(x => x.Id == optionId), Is.False);
    }
}
