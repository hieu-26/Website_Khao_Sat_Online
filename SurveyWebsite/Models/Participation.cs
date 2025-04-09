using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class Participation
{
    public int ParticipationId { get; set; }

    public int SurveyId { get; set; }

    public int? UserId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Survey Survey { get; set; } = null!;

    public virtual User? User { get; set; }
}
