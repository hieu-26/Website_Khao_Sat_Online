using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class Survey
{
    public int SurveyId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public int CreatorUserId { get; set; }

    public bool IsPublic { get; set; }

    public virtual User CreatorUser { get; set; } = null!;

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<SurveyAllowedUser> SurveyAllowedUsers { get; set; } = new List<SurveyAllowedUser>();

    public virtual SurveySetting? SurveySetting { get; set; }
}
