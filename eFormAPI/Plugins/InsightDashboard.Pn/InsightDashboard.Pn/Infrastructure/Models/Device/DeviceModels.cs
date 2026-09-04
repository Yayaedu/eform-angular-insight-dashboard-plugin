namespace InsightDashboard.Pn.Infrastructure.Models.Device;

using System.Collections.Generic;
using Newtonsoft.Json;

// Simplificeret device-sync-kontrakt til lokal test af insight_app mod denne
// backend. Erstatter IKKE den rigtige enhedsparring (OTP/Unit mod Microtings
// cloud) — det er en kendt fremtidig opgave. Her parres direkte på SiteId,
// og token er et in-memory-genereret device-token (se DeviceTokenStore).
//
// Snake_case JsonProperty overalt, da insight_app's AnswerCycle.toJson()
// (Dart) allerede sender/forventer det formatet (form_urlencoded.dart-
// konventionen), uafhængigt af backendens globale camelCase-resolver.
public class DevicePairRequestModel
{
    [JsonProperty("site_id")]
    public int SiteId { get; set; }
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
