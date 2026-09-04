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

namespace InsightDashboard.Pn.Controllers;

using System.Threading.Tasks;
using Infrastructure.Models.Questions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Services.QuestionSetsService;

// Spørgeskema-builderen: CRUD på selve spørgeskemaet (QuestionSet).
// Se QuestionsController/OptionsController for spørgsmål/svarmuligheder.
[Authorize]
public class QuestionSetsController : Controller
{
    private readonly IQuestionSetsService _questionSetsService;

    public QuestionSetsController(IQuestionSetsService questionSetsService)
    {
        _questionSetsService = questionSetsService;
    }

    [HttpGet]
    [Route("api/insight-dashboard-pn/question-sets")]
    public async Task<OperationDataResult<QuestionSetListModel>> Index()
    {
        return await _questionSetsService.Index();
    }

    [HttpGet]
    [Route("api/insight-dashboard-pn/question-sets/{id}")]
    public async Task<OperationDataResult<QuestionSetModel>> Get(int id)
    {
        return await _questionSetsService.Get(id);
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/question-sets/create")]
    public async Task<OperationDataResult<int>> Create([FromBody] QuestionSetCreateModel createModel)
    {
        return await _questionSetsService.Create(createModel);
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/question-sets/update")]
    public async Task<OperationResult> Update([FromBody] QuestionSetUpdateModel updateModel)
    {
        return await _questionSetsService.Update(updateModel);
    }

    [HttpDelete]
    [Route("api/insight-dashboard-pn/question-sets/{id}")]
    public async Task<OperationResult> Delete(int id)
    {
        return await _questionSetsService.Delete(id);
    }
}
