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
using Services.OptionsService;

// Kun relevant for BUTTONS/LIST/MULTI-spørgsmål — se OptionsService.
[Authorize]
public class OptionsController : Controller
{
    private readonly IOptionsService _optionsService;

    public OptionsController(IOptionsService optionsService)
    {
        _optionsService = optionsService;
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/options/create")]
    public async Task<OperationDataResult<int>> Create([FromBody] OptionCreateModel createModel)
    {
        return await _optionsService.Create(createModel);
    }

    [HttpPost]
    [Route("api/insight-dashboard-pn/options/update")]
    public async Task<OperationResult> Update([FromBody] OptionUpdateModel updateModel)
    {
        return await _optionsService.Update(updateModel);
    }

    [HttpDelete]
    [Route("api/insight-dashboard-pn/options/{id}")]
    public async Task<OperationResult> Delete(int id)
    {
        return await _optionsService.Delete(id);
    }
}
