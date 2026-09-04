namespace InsightDashboard.Pn.Infrastructure.Models.Questions;

using System.Collections.Generic;

// Spørgeskema-builderen. Rækkefølgen er LINEÆR (bekræftet af René) — intet
// next_question_id, kun QuestionIndex/OptionIndex som allerede findes i
// den underliggende Microting.eForm-SDK.
//
// De 8 typer der kræver manuel option-håndtering via OptionsController er:
// buttons, list, multi. Alle andre typer (smiley/smiley2-10, text, number,
// text_email, picture, zipcode, info_text) får deres options auto-
// genereret af SDK'en selv når Question.Create(dbContext, true) kaldes —
// se QuestionsService.

public class QuestionSetModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<QuestionModel> Questions { get; set; } = new();
}

public class QuestionSetListModel
{
    public List<QuestionSetModel> Entities { get; set; } = new();
    public int Total { get; set; }
}

public class QuestionSetCreateModel
{
    public string Name { get; set; }
}

public class QuestionSetUpdateModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class QuestionModel
{
    public int Id { get; set; }
    public int QuestionSetId { get; set; }
    public string Text { get; set; }
    public string QuestionType { get; set; }
    public int QuestionIndex { get; set; }
    public List<OptionModel> Options { get; set; } = new();
}

public class QuestionCreateModel
{
    public int QuestionSetId { get; set; }
    public string Text { get; set; }
    public string QuestionType { get; set; }
}

public class QuestionUpdateModel
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string QuestionType { get; set; }
}

// Flytter et spørgsmål til et nyt index — de øvrige spørgsmål i sættet
// rykkes automatisk for at holde QuestionIndex sammenhængende (0..n-1).
public class QuestionReorderModel
{
    public int Id { get; set; }
    public int NewIndex { get; set; }
}

public class OptionModel
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Label { get; set; }
    public int OptionIndex { get; set; }
}

public class OptionCreateModel
{
    public int QuestionId { get; set; }
    public string Label { get; set; }
}

public class OptionUpdateModel
{
    public int Id { get; set; }
    public string Label { get; set; }
}
