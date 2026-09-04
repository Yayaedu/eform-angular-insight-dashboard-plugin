namespace InsightDashboard.Pn.Services.QuestionsService;

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

// Spørgsmåls-laget i builderen. Rækkefølgen er QuestionIndex — lineær,
// intet next_question_id (bekræftet af René).
//
// VIGTIGT om options: for de fleste typer (smiley/smiley2-10, text,
// number, text_email, picture, zipcode, info_text) genererer selve
// Microting.eForm-SDK'en automatisk de rigtige options, når man kalder
// Question.Create(dbContext, true) — se SDK'ens GenerateSpecialQuestionTypes.
// Kun buttons/list/multi kræver at admin selv opretter options via
// IOptionsService. Der skal IKKE bygges egen options-generering for de
// andre typer — det ville duplikere/konfliktere med SDK'ens egen logik.
//
// Sprog: hardkodet til languageId 1 i denne første version, samme
// konvention som SurveysService.AddTextAnswers/SDK'ens egen
// GenerateSpecialQuestionTypes(dbContext, 1) allerede bruger i dette
// projekt. Skal udvides hvis flersprogede spørgeskemaer bliver et krav.
public class QuestionsService : IQuestionsService
{
    private const int DefaultLanguageId = 1;

    private readonly ILogger<QuestionsService> _logger;
    private readonly IInsightDashboardLocalizationService _localizationService;
    private readonly IEFormCoreService _coreHelper;

    public QuestionsService(
        ILogger<QuestionsService> logger,
        IInsightDashboardLocalizationService localizationService,
        IEFormCoreService coreHelper)
    {
        _logger = logger;
        _localizationService = localizationService;
        _coreHelper = coreHelper;
    }

    public async Task<OperationDataResult<int>> Create(QuestionCreateModel createModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var questionSetExists = await sdkContext.QuestionSets
                .Where(x => x.Id == createModel.QuestionSetId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .AnyAsync();
            if (!questionSetExists)
            {
                return new OperationDataResult<int>(false, _localizationService.GetString("QuestionSetNotFound"));
            }

            var nextIndex = await sdkContext.Questions
                .Where(x => x.QuestionSetId == createModel.QuestionSetId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Select(x => (int?)x.QuestionIndex)
                .MaxAsync() ?? -1;
            nextIndex += 1;

            var question = new Question
            {
                QuestionSetId = createModel.QuestionSetId,
                QuestionType = createModel.QuestionType,
                QuestionIndex = nextIndex,
            };
            // true = lad SDK'en generere de rigtige special-options for
            // denne type (smiley-trin, text/number-placeholders osv.).
            await question.Create(sdkContext, true);

            await new QuestionTranslation
            {
                QuestionId = question.Id,
                LanguageId = DefaultLanguageId,
                Name = createModel.Text,
            }.Create(sdkContext);

            return new OperationDataResult<int>(true, question.Id);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<int>(false, _localizationService.GetString("ErrorWhileCreatingQuestion"));
        }
    }

    public async Task<OperationResult> Update(QuestionUpdateModel updateModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var question = await sdkContext.Questions
                .Where(x => x.Id == updateModel.Id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();
            if (question == null)
            {
                return new OperationResult(false, _localizationService.GetString("QuestionNotFound"));
            }

            // OBS: ændring af QuestionType her regenererer IKKE special-
            // options — hvis typen ændres til/fra en auto-genereret type,
            // skal spørgsmålet slettes og genoprettes i denne version.
            question.QuestionType = updateModel.QuestionType;
            await question.Update(sdkContext);

            var translation = await sdkContext.QuestionTranslations
                .Where(x => x.QuestionId == question.Id)
                .Where(x => x.LanguageId == DefaultLanguageId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();

            if (translation == null)
            {
                await new QuestionTranslation
                {
                    QuestionId = question.Id,
                    LanguageId = DefaultLanguageId,
                    Name = updateModel.Text,
                }.Create(sdkContext);
            }
            else
            {
                translation.Name = updateModel.Text;
                await translation.Update(sdkContext);
            }

            return new OperationResult(true, _localizationService.GetString("QuestionUpdatedSuccessfully"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileUpdatingQuestion"));
        }
    }

    public async Task<OperationResult> Reorder(QuestionReorderModel reorderModel)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var question = await sdkContext.Questions
                .Where(x => x.Id == reorderModel.Id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();
            if (question == null)
            {
                return new OperationResult(false, _localizationService.GetString("QuestionNotFound"));
            }

            var siblings = await sdkContext.Questions
                .Where(x => x.QuestionSetId == question.QuestionSetId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Where(x => x.Id != question.Id)
                .OrderBy(x => x.QuestionIndex)
                .ToListAsync();

            var newIndex = Math.Clamp(reorderModel.NewIndex, 0, siblings.Count);
            siblings.Insert(newIndex, question);

            // Genskriv QuestionIndex 0..n-1 i den nye rækkefølge — holder
            // indexet sammenhængende, som resten af koden regner med.
            for (var i = 0; i < siblings.Count; i++)
            {
                if (siblings[i].QuestionIndex != i)
                {
                    siblings[i].QuestionIndex = i;
                    await siblings[i].Update(sdkContext);
                }
            }

            return new OperationResult(true, _localizationService.GetString("QuestionReorderedSuccessfully"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileReorderingQuestion"));
        }
    }

    public async Task<OperationResult> Delete(int id)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var question = await sdkContext.Questions
                .Where(x => x.Id == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();
            if (question == null)
            {
                return new OperationResult(false, _localizationService.GetString("QuestionNotFound"));
            }

            var options = await sdkContext.Options
                .Where(x => x.QuestionId == id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .ToListAsync();
            foreach (var option in options)
            {
                await option.Delete(sdkContext);
            }

            await question.Delete(sdkContext);

            return new OperationResult(true, _localizationService.GetString("QuestionHasBeenRemoved"));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationResult(false, _localizationService.GetString("ErrorWhileRemovingQuestion"));
        }
    }
}
