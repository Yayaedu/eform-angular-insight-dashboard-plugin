namespace InsightDashboard.Pn.Services.DeviceSyncService;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.InsightDashboardLocalizationService;
using Infrastructure.Models.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

// Lokal-test-genvej: parrer direkte på SiteId (ingen OTP/Unit-cloud-
// handshake), henter spørgeskemaet der er koblet til sitet via
// SurveyConfiguration, og gemmer besvarelser i de rigtige SDK-tabeller
// (Answer/AnswerValue) — samme tabeller "Answers"-admin-siden læser fra.
public class DeviceSyncService : IDeviceSyncService
{
    private readonly ILogger<DeviceSyncService> _logger;
    private readonly IInsightDashboardLocalizationService _localizationService;
    private readonly IEFormCoreService _coreHelper;
    private readonly IDeviceTokenStore _tokenStore;

    public DeviceSyncService(
        ILogger<DeviceSyncService> logger,
        IInsightDashboardLocalizationService localizationService,
        IEFormCoreService coreHelper,
        IDeviceTokenStore tokenStore)
    {
        _logger = logger;
        _localizationService = localizationService;
        _coreHelper = coreHelper;
        _tokenStore = tokenStore;
    }

    public async Task<OperationDataResult<DevicePairResponseModel>> Pair(int siteMicrotingUid)
    {
        try
        {
            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            // siteMicrotingUid er det id "Device Users"-siden viser til
            // admins — Sites.Id (den interne PK) er skjult i UI'et.
            var site = await sdkContext.Sites
                .Where(x => x.MicrotingUid == siteMicrotingUid)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();

            if (site == null)
            {
                return new OperationDataResult<DevicePairResponseModel>(false,
                    _localizationService.GetString("SiteNotFound"));
            }

            var token = _tokenStore.CreateToken(site.Id);

            return new OperationDataResult<DevicePairResponseModel>(true, new DevicePairResponseModel
            {
                Token = token,
                SiteId = site.Id,
                SiteName = site.Name,
            });
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<DevicePairResponseModel>(false,
                _localizationService.GetString("ErrorWhilePairingDevice"));
        }
    }

    public async Task<OperationDataResult<DeviceQuestionSetResponseModel>> GetQuestionSetForToken(string token)
    {
        try
        {
            if (!_tokenStore.TryGetSiteId(token, out var siteId))
            {
                return new OperationDataResult<DeviceQuestionSetResponseModel>(false,
                    _localizationService.GetString("DeviceNotPaired"));
            }

            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var site = await sdkContext.Sites
                .Where(x => x.Id == siteId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var surveyConfigurationId = await sdkContext.SiteSurveyConfigurations
                .Where(x => x.SiteId == siteId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Select(x => x.SurveyConfigurationId)
                .FirstOrDefaultAsync();

            if (surveyConfigurationId == 0)
            {
                return new OperationDataResult<DeviceQuestionSetResponseModel>(false,
                    _localizationService.GetString("NoSurveyAssignedToSite"));
            }

            var questionSetId = await sdkContext.SurveyConfigurations
                .Where(x => x.Id == surveyConfigurationId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Select(x => x.QuestionSetId)
                .FirstOrDefaultAsync();

            var questionSet = await sdkContext.QuestionSets
                .Where(x => x.Id == questionSetId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (questionSet == null)
            {
                return new OperationDataResult<DeviceQuestionSetResponseModel>(false,
                    _localizationService.GetString("QuestionSetNotFound"));
            }

            var languageId = site?.LanguageId > 0 ? site.LanguageId : 1;

            var model = new DeviceQuestionSetResponseModel
            {
                Id = questionSet.Id,
                Name = questionSet.Name,
                SurveyConfigurationId = surveyConfigurationId,
                Languages = { new DeviceLanguageModel { Id = languageId, Priority = 0 } },
            };

            var questions = await sdkContext.Questions
                .Where(x => x.QuestionSetId == questionSet.Id)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .OrderBy(x => x.QuestionIndex)
                .AsNoTracking()
                .ToListAsync();

            foreach (var question in questions)
            {
                var text = await sdkContext.QuestionTranslations
                    .Where(x => x.QuestionId == question.Id)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();

                var questionModel = new DeviceQuestionResponseModel
                {
                    Id = question.Id,
                    QuestionType = question.QuestionType,
                    Text = text,
                };

                var options = await sdkContext.Options
                    .Where(x => x.QuestionId == question.Id)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .OrderBy(x => x.OptionIndex)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var option in options)
                {
                    var label = await sdkContext.OptionTranslations
                        .Where(x => x.OptionId == option.Id)
                        .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                    questionModel.Options.Add(new DeviceOptionResponseModel
                    {
                        Id = option.Id,
                        Label = label,
                    });
                }

                model.Questions.Add(questionModel);
            }

            return new OperationDataResult<DeviceQuestionSetResponseModel>(true, model);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<DeviceQuestionSetResponseModel>(false,
                _localizationService.GetString("ErrorWhileObtainingQuestionSet"));
        }
    }

    public async Task<OperationDataResult<DeviceAnswerSubmitResponseModel>> SubmitAnswerCycle(string token, DeviceAnswerCycleModel cycleModel)
    {
        try
        {
            if (!_tokenStore.TryGetSiteId(token, out var siteId))
            {
                return new OperationDataResult<DeviceAnswerSubmitResponseModel>(false,
                    _localizationService.GetString("DeviceNotPaired"));
            }

            var core = await _coreHelper.GetCore();
            await using var sdkContext = core.DbContextHelper.GetDbContext();

            var finishedAt = DateTime.TryParse(cycleModel.FinishedAt, null, DateTimeStyles.AdjustToUniversal,
                out var parsed)
                ? parsed
                : DateTime.UtcNow;

            var totalDurationMs = cycleModel.Answers.Sum(a => a.QuestionDuration);

            var answer = new Answer
            {
                SiteId = siteId,
                UnitId = null,
                AnswerDuration = totalDurationMs / 1000,
                LanguageId = cycleModel.LanguageId > 0 ? cycleModel.LanguageId : 1,
                SurveyConfigurationId = cycleModel.SurveyConfigurationId,
                FinishedAt = finishedAt,
                QuestionSetId = int.TryParse(cycleModel.QuestionSetId, out var qsId) ? qsId : 0,
                UtcAdjusted = true,
                TimeZone = "UTC",
            };
            await answer.Create(sdkContext);

            // MicrotingUid er nøglen "Answers"-admin-siden slår op på — sæt
            // den til Answer.Id (unik, kendt først efter Create) så single-
            // record-opslag/-sletning også virker for enheds-indsendte svar.
            answer.MicrotingUid = answer.Id;
            await answer.Update(sdkContext);

            foreach (var item in cycleModel.Answers)
            {
                if (!int.TryParse(item.QuestionId, out var questionId))
                {
                    continue;
                }

                if (item.AnswerValues.Count == 0)
                {
                    continue;
                }

                for (var i = 0; i < item.AnswerValues.Count; i++)
                {
                    if (!int.TryParse(item.AnswerValues[i], out var optionId))
                    {
                        continue;
                    }

                    var answerValue = new AnswerValue
                    {
                        AnswerId = answer.Id,
                        QuestionId = questionId,
                        OptionId = optionId,
                        Value = i == 0 && item.TextValues.Count > 0 ? item.TextValues[0] : null,
                    };
                    await answerValue.Create(sdkContext);
                }
            }

            return new OperationDataResult<DeviceAnswerSubmitResponseModel>(true,
                new DeviceAnswerSubmitResponseModel { UnitAnswerId = cycleModel.UnitAnswerId });
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            _logger.LogError(e.Message);
            return new OperationDataResult<DeviceAnswerSubmitResponseModel>(false,
                _localizationService.GetString("ErrorWhileSubmittingAnswerCycle"));
        }
    }
}
