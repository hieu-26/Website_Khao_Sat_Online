using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class Option
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Question Question { get; set; } = null!;
}
