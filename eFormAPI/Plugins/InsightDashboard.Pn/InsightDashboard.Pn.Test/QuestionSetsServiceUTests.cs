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

// Dækker den service-til-database-vej, der tidligere ikke fandtes et
// eksempel på i dette projekt: IEFormCoreService -> Core -> rigtig SDK-
// database. Se InsightCoreTestHelper for hvordan det bygges uden Core.Start().
[TestFixture]
public class QuestionSetsServiceUTests : DbTestFixture
{
    private QuestionSetsService _questionSetsService;
    private QuestionsService _questionsService;
    private OptionsService _optionsService;

    protected override void DoSetup()
    {
        // QuestionTranslation/OptionTranslation har en FK mod Languages.Id —
        // en frisk testdatabase har ingen sprog seedet, så uden dette fejler
        // enhver Create() på et spørgsmål/option med en constraint-fejl der
        // fanges af servicens try/catch og viser sig som et forvirrende
        // "Success = false" uden synlig årsag.
        Microting.eForm.Infrastructure.Data.Entities.Language.AddDefaultLanguages(DbContext).GetAwaiter().GetResult();

        var coreHelper = InsightCoreTestHelper.BuildFakeCoreService(ConnectionString);
        var localizationService = MockHelper.GetLocalizationService();

        _questionSetsService = new QuestionSetsService(
            Substitute.For<ILogger<QuestionSetsService>>(), localizationService, coreHelper);
        _questionsService = new QuestionsService(
            Substitute.For<ILogger<QuestionsService>>(), localizationService, coreHelper);
        _optionsService = new OptionsService(
            Substitute.For<ILogger<OptionsService>>(), localizationService, coreHelper);
    }

    [Test]
    public async Task Create_ThenGet_ReturnsQuestionSetWithName()
    {
        var createResult = await _questionSetsService.Create(new QuestionSetCreateModel { Name = "UTest set" });

        Assert.That(createResult.Success, Is.True);
        var id = createResult.Model;

        var getResult = await _questionSetsService.Get(id);

        Assert.That(getResult.Success, Is.True);
        Assert.That(getResult.Model.Name, Is.EqualTo("UTest set"));
        Assert.That(getResult.Model.Questions, Is.Empty);

        await _questionSetsService.Delete(id);
    }

    [Test]
    public async Task Get_IncludesQuestionsAndOptions_InIndexOrder()
    {
        var setId = (await _questionSetsService.Create(new QuestionSetCreateModel { Name = "UTest set with content" })).Model;

        var buttonsQuestionId = (await _questionsService.Create(new QuestionCreateModel
        {
            QuestionSetId = setId,
            QuestionType = "buttons",
            Text = "Hvordan var oplevelsen?",
        })).Model;

        await _optionsService.Create(new OptionCreateModel { QuestionId = buttonsQuestionId, Label = "Godt" });
        await _optionsService.Create(new OptionCreateModel { QuestionId = buttonsQuestionId, Label = "Dårligt" });

        var textQuestionId = (await _questionsService.Create(new QuestionCreateModel
        {
            QuestionSetId = setId,
            QuestionType = "text",
            Text = "Uddyb gerne",
        })).Model;

        var getResult = await _questionSetsService.Get(setId);

        Assert.That(getResult.Success, Is.True);
        Assert.That(getResult.Model.Questions, Has.Count.EqualTo(2));

        var buttonsQuestion = getResult.Model.Questions.Single(x => x.Id == buttonsQuestionId);
        Assert.That(buttonsQuestion.Text, Is.EqualTo("Hvordan var oplevelsen?"));
        Assert.That(buttonsQuestion.QuestionIndex, Is.EqualTo(0));
        Assert.That(buttonsQuestion.Options.Select(o => o.Label), Is.EqualTo(new[] { "Godt", "Dårligt" }));

        var textQuestion = getResult.Model.Questions.Single(x => x.Id == textQuestionId);
        Assert.That(textQuestion.QuestionIndex, Is.EqualTo(1));
        // "text" auto-genereres af SDK'en (se QuestionsService) — ét
        // placeholder-option, ikke tomt.
        Assert.That(textQuestion.Options, Is.Not.Empty);

        await _questionSetsService.Delete(setId);
    }

    [Test]
    public async Task Update_ChangesName()
    {
        var setId = (await _questionSetsService.Create(new QuestionSetCreateModel { Name = "Before" })).Model;

        var updateResult = await _questionSetsService.Update(new QuestionSetUpdateModel { Id = setId, Name = "After" });
        Assert.That(updateResult.Success, Is.True);

        var getResult = await _questionSetsService.Get(setId);
        Assert.That(getResult.Model.Name, Is.EqualTo("After"));

        await _questionSetsService.Delete(setId);
    }

    [Test]
    public async Task Delete_IsSoftDelete_NotReturnedByGetAfterwards()
    {
        var setId = (await _questionSetsService.Create(new QuestionSetCreateModel { Name = "To be removed" })).Model;

        var deleteResult = await _questionSetsService.Delete(setId);
        Assert.That(deleteResult.Success, Is.True);

        var getResult = await _questionSetsService.Get(setId);
        Assert.That(getResult.Success, Is.False);
    }

    [Test]
    public async Task Get_UnknownId_ReturnsFailure()
    {
        var getResult = await _questionSetsService.Get(int.MaxValue);

        Assert.That(getResult.Success, Is.False);
    }
}
