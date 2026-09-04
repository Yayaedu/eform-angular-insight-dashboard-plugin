namespace InsightDashboard.Pn.Services.QuestionsService;

using System.Threading.Tasks;
using Infrastructure.Models.Questions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

public interface IQuestionsService
{
    Task<OperationDataResult<int>> Create(QuestionCreateModel createModel);
    Task<OperationResult> Update(QuestionUpdateModel updateModel);
    Task<OperationResult> Reorder(QuestionReorderModel reorderModel);
    Task<OperationResult> Delete(int id);
}
