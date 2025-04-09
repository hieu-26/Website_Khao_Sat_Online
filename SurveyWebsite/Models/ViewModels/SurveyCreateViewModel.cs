namespace SurveyWebsite.Models.ViewModels
{
    public class SurveyCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public bool AllowMultipleResponses { get; set; }
        public bool RequireLogin { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class QuestionViewModel
    {
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public bool IsRequired { get; set; }
        public List<OptionViewModel> Options { get; set; } = new();
    }

    public class OptionViewModel
    {
        public string OptionText { get; set; }
    }
}
