namespace InsightDashboard.Pn.Services.OptionsService;

using System.Threading.Tasks;
using Infrastructure.Models.Questions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

public interface IOptionsService
{
    Task<OperationDataResult<int>> Create(OptionCreateModel createModel);
    Task<OperationResult> Update(OptionUpdateModel updateModel);
    Task<OperationResult> Delete(int id);
}
