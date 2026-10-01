namespace InsightDashboard.Pn.Controllers;

using System.Threading.Tasks;
using Infrastructure.Models.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Services.DeviceSyncService;

// Admin-facing modstykke til DeviceController: kræver login (i modsætning
// til den anonyme tablet-API), og bruges til at generere/regenerere den
// engangskode en admin taster ind på insight_app for at parre den med et
// site. Holdt i en separat controller fordi [AllowAnonymous] på klasse-
// niveau i ASP.NET Core ikke kan overstyres af [Authorize] på en enkelt
// action i samme controller.
[Authorize]
[Route("api/insight-dashboard-pn/device-pairing")]
public class DevicePairingController : Controller
{
    private readonly IDeviceSyncService _deviceSyncService;

    public DevicePairingController(IDeviceSyncService deviceSyncService)
    {
        _deviceSyncService = deviceSyncService;
    }

    [HttpPost]
    [Route("code")]
    public async Task<OperationDataResult<DevicePairingCodeResponseModel>> RequestPairingCode(
        [FromBody] DevicePairingCodeRequestModel model)
    {
        return await _deviceSyncService.RequestPairingCode(model.SiteId);
    }
}
