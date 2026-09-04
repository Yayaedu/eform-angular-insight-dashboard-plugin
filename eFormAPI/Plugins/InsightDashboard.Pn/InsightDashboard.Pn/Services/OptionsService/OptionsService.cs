namespace InsightDashboard.Pn.Services.OptionsService;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.InsightDashboardLocalizationService;
using Infrastructure.Models.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

// Kun relevant for BUTTONS/LIST/MULTI — de øvrige typer får deres options
// auto-genereret af SDK'en (se QuestionsService). Bruges IKKE til at
// oprette options på auto-genererede typer; det ville give dubletter.
public class OptionsService : IOptionsService
{
    private const int DefaultLanguageId = 1;

    private readonly ILogger<OptionsService> _logger;
    private readonly IInsightDashboardLocalizationService _localizationService;
    private readonly IEFormCoreService _coreHelper;

    public OptionsService(
        ILogger<OptionsService> logger,
        IInsightDashboardLocalizationService localizationService,
        IEFormCoreService coreHelper)
    {
        _logger = logger;
        _localizationService = localizationService;
        _coreHelper = coreHelper;
    }

    public async Task<OperationDataResult<int>> Create(OptionCreateModel createModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionExists = await sdkContext.Questions
                .Where(x => x.Id == createModel.QuestionId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .AnyAsync();
            if (!questionExists)
            {
                return new OperationDataResult<int>(false, _localizationService.GetString("QuestionNotFound"));
            }

            var nextIndex = await sdkContext.Options
                .Where(x => x.QuestionId == createModel.QuestionId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Select(x => (int?)x.OptionIndex)
                .MaxAsync() ?? -1;
            nextIndex += 1;

            var option = new Option
            {
                QuestionId = createModel.QuestionId,
                OptionIndex = nextIndex,
            };
            await option.Create(sdkContext);

            await new OptionTranslation
            {
                OptionId = option.Id,
                LanguageId = DefaultLanguageId,
                Name = createModel.Label,
            }.Create(sdkContext);

            return new OperationDataResult<int>(true, option.Id);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<int>(false, _localizationService.GetString("ErrorWhileCreatingOption"));
        }
    }

    public async Task<OperationResult> Update(OptionUpdateModel updateModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var option = await sdkContext.Options
                .Where(x => x.Id == updateModel.Id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();
            if (option == null)
            {
                return new OperationResult(false, _localizationService.GetString("OptionNotFound"));
            }

            var translation = await sdkContext.OptionTranslations
                .Where(x => x.OptionId == option.Id)
                .Where(x => x.LanguageId == DefaultLanguageId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();

            if (translation == null)
            {
                await new OptionTranslation
                {
                    OptionId = option.Id,
                    LanguageId = DefaultLanguageId,
                    Name = updateModel.Label,
                }.Create(sdkContext);
            }
            else
            {
                translation.Name = updateModel.Label;
                await translation.Update(sdkContext);
            }

            return new OperationResult(true, _localizationService.GetString("OptionUpdatedSuccessfully"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileUpdatingOption"));
        }
    }

    public async Task<OperationResult> Delete(int id)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var option = await sdkContext.Options
                .Where(x => x.Id == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();
            if (option == null)
            {
                return new OperationResult(false, _localizationService.GetString("OptionNotFound"));
            }

            await option.Delete(sdkContext);

            return new OperationResult(true, _localizationService.GetString("OptionHasBeenRemoved"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileRemovingOption"));
        }
    }
}
