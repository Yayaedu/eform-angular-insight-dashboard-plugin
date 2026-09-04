namespace InsightDashboard.Pn.Services.QuestionSetsService;

using System.Threading.Tasks;
using Infrastructure.Models.Questions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

public interface IQuestionSetsService
{
    Task<OperationDataResult<QuestionSetListModel>> Index();
    Task<OperationDataResult<QuestionSetModel>> Get(int id);
    Task<OperationDataResult<int>> Create(QuestionSetCreateModel createModel);
    Task<OperationResult> Update(QuestionSetUpdateModel updateModel);
    Task<OperationResult> Delete(int id);
}
