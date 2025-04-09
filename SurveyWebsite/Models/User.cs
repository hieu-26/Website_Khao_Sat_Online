using System;
using System.Collections.Generic;

namespace SurveyWebsite.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<SurveyAllowedUser> SurveyAllowedUsers { get; set; } = new List<SurveyAllowedUser>();

    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}
