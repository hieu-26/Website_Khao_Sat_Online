using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class SurveySetting
{
    public int SurveyId { get; set; }

    public bool AllowMultipleResponses { get; set; }

    public bool RequireLogin { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Survey Survey { get; set; } = null!;
}
