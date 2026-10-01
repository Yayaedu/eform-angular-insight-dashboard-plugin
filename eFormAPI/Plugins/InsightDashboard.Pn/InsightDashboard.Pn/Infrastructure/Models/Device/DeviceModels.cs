namespace InsightDashboard.Pn.Infrastructure.Models.Device;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// Device-sync-kontrakt til insight_app. Parring sker nu via en tidsbegrænset
// engangskode (se DevicePairingCodeStore), ikke længere direkte på SiteId —
// koden genereres af en admin (DevicePairingController) og tastes ind i
// appen, samme UX som eform-angular-frontends rigtige Unit-OTP-flow
// (Device Users-siden, "New OTP"). Selve valideringen sker dog lokalt i
// pluginet, da insight_app ikke kan gennemføre Microtings cloud-handshake.
// Token efter parring er fortsat et in-memory device-token (DeviceTokenStore).
//
// Snake_case JsonProperty overalt, da insight_app's AnswerCycle.toJson()
// (Dart) allerede sender/forventer det formatet (form_urlencoded.dart-
// konventionen), uafhængigt af backendens globale camelCase-resolver.
public class DevicePairRequestModel
{
    [JsonProperty("code")]
    public string Code { get; set; }
}

public class DevicePairResponseModel
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("site_id")]
    public int SiteId { get; set; }

    [JsonProperty("site_name")]
    public string SiteName { get; set; }
}

// Admin-facing: anmoder om (eller regenererer) en parringskode for et site.
// SiteId her er, ligesom resten af device-API'et, Sites.MicrotingUid.
public class DevicePairingCodeRequestModel
{
    [JsonProperty("site_id")]
    public int SiteId { get; set; }
}

public class DevicePairingCodeResponseModel
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("site_id")]
    public int SiteId { get; set; }

    [JsonProperty("site_name")]
    public string SiteName { get; set; }

    [JsonProperty("expires_at")]
    public DateTime ExpiresAtUtc { get; set; }
}

public class DeviceAnswerItemModel
{
    [JsonProperty("question_id")]
    public string QuestionId { get; set; }

    [JsonProperty("answer_values")]
    public List<string> AnswerValues { get; set; } = new();

    [JsonProperty("text_values")]
    public List<string> TextValues { get; set; } = new();

    [JsonProperty("question_duration")]
    public int QuestionDuration { get; set; }
}

public class DeviceAnswerCycleModel
{
    [JsonProperty("unit_answer_id")]
    public string UnitAnswerId { get; set; }

    [JsonProperty("question_set_id")]
    public string QuestionSetId { get; set; }

    [JsonProperty("survey_configuration_id")]
    public int SurveyConfigurationId { get; set; }

    [JsonProperty("language_id")]
    public int LanguageId { get; set; }

    [JsonProperty("finished_at")]
    public string FinishedAt { get; set; }

    [JsonProperty("answers")]
    public List<DeviceAnswerItemModel> Answers { get; set; } = new();
}

public class DeviceAnswerSubmitResponseModel
{
    [JsonProperty("unit_answer_id")]
    public string UnitAnswerId { get; set; }
}
