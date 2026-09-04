namespace InsightDashboard.Pn.Infrastructure.Models.Device;

using System.Collections.Generic;
using Newtonsoft.Json;

// Match insight_app's QuestionSet.fromJson (Flutter) felt-for-felt — bevidst
// snake_case uanset den globale CamelCasePropertyNamesContractResolver, da
// dette er en device-facing kontrakt, ikke admin-API'et.
public class DeviceQuestionSetResponseModel
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("survey_configuration_id")]
    public int SurveyConfigurationId { get; set; }

    [JsonProperty("languages")]
    public List<DeviceLanguageModel> Languages { get; set; } = new();

    [JsonProperty("questions")]
    public List<DeviceQuestionResponseModel> Questions { get; set; } = new();
}

public class DeviceLanguageModel
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("priority")]
    public int Priority { get; set; }
}

public class DeviceQuestionResponseModel
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("question_type")]
    public string QuestionType { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("options")]
    public List<DeviceOptionResponseModel> Options { get; set; } = new();
}

public class DeviceOptionResponseModel
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }
}
