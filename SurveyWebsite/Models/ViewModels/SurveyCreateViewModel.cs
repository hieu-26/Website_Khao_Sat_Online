using System.ComponentModel.DataAnnotations;

namespace SurveyWebsite.Models.ViewModels
{
    public class SurveyCreateViewModel
    {
        public int SurveyId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPublic { get; set; }

        public bool AllowMultipleResponses { get; set; }

        public bool RequireLogin { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();

        public List<int> AllowedUserIds { get; set; } = new List<int>();

        public List<User> AllUsers { get; set; } = new List<User>(); // ✅ KHỞI TẠO MẶC ĐỊNH


        public class QuestionViewModel
        {
            public int QuestionId { get; set; }

            [Required]
            public string QuestionText { get; set; }

            public string QuestionType { get; set; }

            public bool IsRequired { get; set; }

            public List<OptionViewModel> Options { get; set; } = new();
        }

        public class OptionViewModel
        {
            public int OptionId { get; set; }

            [Required]
            public string OptionText { get; set; }
        }

    }
}