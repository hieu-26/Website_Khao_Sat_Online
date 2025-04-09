using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class SurveyAllowedUser
{
    public int SurveyId { get; set; }

    public int UserId { get; set; }

    public DateTime AddedDate { get; set; }

    public virtual Survey Survey { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
