namespace InsightDashboard.Pn.Services.QuestionSetsService;

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

// Spørgeskema-builderens QuestionSets-lag. Rene CRUD-operationer på selve
// "spørgeskemaet" — Questions/Options håndteres af deres egne services.
public class QuestionSetsService : IQuestionSetsService
{
    private readonly ILogger<QuestionSetsService> _logger;
    private readonly IInsightDashboardLocalizationService _localizationService;
    private readonly IEFormCoreService _coreHelper;

    public QuestionSetsService(
        ILogger<QuestionSetsService> logger,
        IInsightDashboardLocalizationService localizationService,
        IEFormCoreService coreHelper)
    {
        _logger = logger;
        _localizationService = localizationService;
        _coreHelper = coreHelper;
    }

    public async Task<OperationDataResult<QuestionSetListModel>> Index()
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionSets = await sdkContext.QuestionSets
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .AsNoTracking()
                .Select(x => new QuestionSetModel
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .ToListAsync();

            return new OperationDataResult<QuestionSetListModel>(true,
                new QuestionSetListModel { Entities = questionSets, Total = questionSets.Count });
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<QuestionSetListModel>(false,
                _localizationService.GetString("ErrorWhileObtainingQuestionSets"));
        }
    }

    public async Task<OperationDataResult<QuestionSetModel>> Get(int id)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionSet = await sdkContext.QuestionSets
                .Where(x => x.Id == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (questionSet == null)
            {
                return new OperationDataResult<QuestionSetModel>(false,
                    _localizationService.GetString("QuestionSetNotFound"));
            }

            var questions = await sdkContext.Questions
                .Where(x => x.QuestionSetId == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .OrderBy(x => x.QuestionIndex)
                .AsNoTracking()
                .ToListAsync();

            var model = new QuestionSetModel { Id = questionSet.Id, Name = questionSet.Name };

            foreach (var question in questions)
            {
                var text = await sdkContext.QuestionTranslations
                    .Where(x => x.QuestionId == question.Id)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();

                var options = await sdkContext.Options
                    .Where(x => x.QuestionId == question.Id)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .OrderBy(x => x.OptionIndex)
                    .AsNoTracking()
                    .ToListAsync();

                var optionModels = new System.Collections.Generic.List<OptionModel>();
                foreach (var option in options)
                {
                    var label = await sdkContext.OptionTranslations
                        .Where(x => x.OptionId == option.Id)
                        .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                    optionModels.Add(new OptionModel
                    {
                        Id = option.Id,
                        QuestionId = option.QuestionId,
                        Label = label,
                        OptionIndex = option.OptionIndex,
                    });
                }

                model.Questions.Add(new QuestionModel
                {
                    Id = question.Id,
                    QuestionSetId = question.QuestionSetId,
                    Text = text,
                    QuestionType = question.QuestionType,
                    QuestionIndex = question.QuestionIndex,
                    Options = optionModels,
                });
            }

            return new OperationDataResult<QuestionSetModel>(true, model);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<QuestionSetModel>(false,
                _localizationService.GetString("ErrorWhileObtainingQuestionSet"));
        }
    }

    public async Task<OperationDataResult<int>> Create(QuestionSetCreateModel createModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionSet = new QuestionSet
            {
                Name = createModel.Name,
            };
            await questionSet.Create(sdkContext);

            return new OperationDataResult<int>(true, questionSet.Id);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<int>(false,
                _localizationService.GetString("ErrorWhileCreatingQuestionSet"));
        }
    }

    public async Task<OperationResult> Update(QuestionSetUpdateModel updateModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionSet = await sdkContext.QuestionSets
                .Where(x => x.Id == updateModel.Id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();

            if (questionSet == null)
            {
                return new OperationResult(false, _localizationService.GetString("QuestionSetNotFound"));
            }

            questionSet.Name = updateModel.Name;
            await questionSet.Update(sdkContext);

            return new OperationResult(true, _localizationService.GetString("QuestionSetUpdatedSuccessfully"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileUpdatingQuestionSet"));
        }
    }

    public async Task<OperationResult> Delete(int id)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionSet = await sdkContext.QuestionSets
                .Where(x => x.Id == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();

            if (questionSet == null)
            {
                return new OperationResult(false, _localizationService.GetString("QuestionSetNotFound"));
            }

            // Soft-slet spørgeskemaet og alle dets spørgsmål/options, så
            // eksisterende besvarelser (som ikke har nogen foreign key til
            // dem) forbliver urørte — samme princip som manualens
            // "svy_answer_cycles has no foreign key at all".
            var questions = await sdkContext.Questions
                .Where(x => x.QuestionSetId == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .ToListAsync();

            foreach (var question in questions)
            {
                var options = await sdkContext.Options
                    .Where(x => x.QuestionId == question.Id)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .ToListAsync();
                foreach (var option in options)
                {
                    await option.Delete(sdkContext);
                }
                await question.Delete(sdkContext);
            }

            await questionSet.Delete(sdkContext);

            return new OperationResult(true, _localizationService.GetString("QuestionSetHasBeenRemoved"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileRemovingQuestionSet"));
        }
    }
}
