using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int SurveyId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public bool IsRequired { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual Survey Survey { get; set; } = null!;
}
