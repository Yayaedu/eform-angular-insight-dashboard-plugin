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
using Services.QuestionsService;
using Services.QuestionSetsService;

[TestFixture]
public class QuestionsServiceUTests : DbTestFixture
{
    private QuestionSetsService _questionSetsService;
    private QuestionsService _questionsService;
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

        _setId = _questionSetsService.Create(new QuestionSetCreateModel { Name = "UTest set" })
            .GetAwaiter().GetResult().Model;
    }

    [TearDown]
    public void CleanupQuestionSet()
    {
        _questionSetsService.Delete(_setId).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Create_AssignsSequentialQuestionIndex()
    {
        var firstId = (await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = _setId, QuestionType = "buttons", Text = "Spørgsmål 1" })).Model;
        var secondId = (await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = _setId, QuestionType = "buttons", Text = "Spørgsmål 2" })).Model;

        var set = (await _questionSetsService.Get(_setId)).Model;

        Assert.That(set.Questions.Single(x => x.Id == firstId).QuestionIndex, Is.EqualTo(0));
        Assert.That(set.Questions.Single(x => x.Id == secondId).QuestionIndex, Is.EqualTo(1));
    }

    [Test]
    public async Task Create_UnknownQuestionSet_ReturnsFailure()
    {
        var result = await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = int.MaxValue, QuestionType = "buttons", Text = "x" });

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task Reorder_MovesQuestionAndRenumbersSiblings()
    {
        var firstId = (await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = _setId, QuestionType = "buttons", Text = "Først" })).Model;
        var secondId = (await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = _setId, QuestionType = "buttons", Text = "Anden" })).Model;
        var thirdId = (await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = _setId, QuestionType = "buttons", Text = "Tredje" })).Model;

        // Flyt det sidste spørgsmål (index 2) til at være det første (index 0).
        var reorderResult = await _questionsService.Reorder(new QuestionReorderModel { Id = thirdId, NewIndex = 0 });
        Assert.That(reorderResult.Success, Is.True);

        var set = (await _questionSetsService.Get(_setId)).Model;
        var ordered = set.Questions.OrderBy(x => x.QuestionIndex).Select(x => x.Id).ToList();

        Assert.That(ordered, Is.EqualTo(new[] { thirdId, firstId, secondId }));
        Assert.That(set.Questions.Select(x => x.QuestionIndex).OrderBy(x => x), Is.EqualTo(new[] { 0, 1, 2 }));
    }

    [Test]
    public async Task Delete_RemovesQuestionAndItsOptions()
    {
        var questionId = (await _questionsService.Create(new QuestionCreateModel
        { QuestionSetId = _setId, QuestionType = "buttons", Text = "Skal slettes" })).Model;

        var deleteResult = await _questionsService.Delete(questionId);
        Assert.That(deleteResult.Success, Is.True);

        var set = (await _questionSetsService.Get(_setId)).Model;
        Assert.That(set.Questions.Any(x => x.Id == questionId), Is.False);
    }
}
