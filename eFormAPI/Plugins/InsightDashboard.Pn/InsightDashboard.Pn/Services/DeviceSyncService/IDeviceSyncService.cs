namespace InsightDashboard.Pn.Services.DeviceSyncService;

using System.Threading.Tasks;
using Infrastructure.Models.Device;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

public interface IDeviceSyncService
{
    Task<OperationDataResult<DevicePairingCodeResponseModel>> RequestPairingCode(int siteId);
    Task<OperationDataResult<DevicePairResponseModel>> Pair(string code);
    Task<OperationDataResult<DeviceQuestionSetResponseModel>> GetQuestionSetForToken(string token);
    Task<OperationDataResult<DeviceAnswerSubmitResponseModel>> SubmitAnswerCycle(string token, DeviceAnswerCycleModel model);
}
