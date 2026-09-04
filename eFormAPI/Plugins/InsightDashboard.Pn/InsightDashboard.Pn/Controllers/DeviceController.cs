namespace InsightDashboard.Pn.Controllers;

using System.Threading.Tasks;
using Infrastructure.Models.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Services.DeviceSyncService;

// Device-facing API til insight_app (Flutter). Bevidst anonym — en fysisk
// tablet har ingen admin-login. Dette er en lokal-test-genvej (parring på
// SiteId, ikke den rigtige OTP/Unit-cloud-parring), se DeviceModels.cs.
[AllowAnonymous]
[Route("api/insight-dashboard-pn/device")]
public class DeviceController : Controller
{
    private readonly IDeviceSyncService _deviceSyncService;

    public DeviceController(IDeviceSyncService deviceSyncService)
    {
        _deviceSyncService = deviceSyncService;
    }

    [HttpPost]
    [Route("pair")]
    public async Task<OperationDataResult<DevicePairResponseModel>> Pair([FromBody] DevicePairRequestModel model)
    {
        return await _deviceSyncService.Pair(model.SiteId);
    }

    [HttpGet]
    [Route("question-set")]
    public async Task<OperationDataResult<DeviceQuestionSetResponseModel>> QuestionSet([FromQuery] string token)
    {
        return await _deviceSyncService.GetQuestionSetForToken(token);
    }

    [HttpPost]
    [Route("answers")]
    public async Task<OperationDataResult<DeviceAnswerSubmitResponseModel>> Answers([FromQuery] string token,
        [FromBody] DeviceAnswerCycleModel model)
    {
        return await _deviceSyncService.SubmitAnswerCycle(token, model);
    }
}
