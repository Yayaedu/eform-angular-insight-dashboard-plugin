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
using Services.QuestionsService;

[Authorize]
public class QuestionsController : Controller
{
    private readonly IQuestionsService _questionsService;

    public QuestionsController(IQuestionsService questionsService)
    {
        _questionsService = questionsService;
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/questions/create")]
    public async Task<OperationDataResult<int>> Create([FromBody] QuestionCreateModel createModel)
    {
        return await _questionsService.Create(createModel);
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/questions/update")]
    public async Task<OperationResult> Update([FromBody] QuestionUpdateModel updateModel)
    {
        return await _questionsService.Update(updateModel);
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/questions/reorder")]
    public async Task<OperationResult> Reorder([FromBody] QuestionReorderModel reorderModel)
    {
        return await _questionsService.Reorder(reorderModel);
    }

    [HttpDelete]
    [Route("api/insight-dashboard-pn/questions/{id}")]
    public async Task<OperationResult> Delete(int id)
    {
        return await _questionsService.Delete(id);
    }
}
