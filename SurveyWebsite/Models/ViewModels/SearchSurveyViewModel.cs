namespace SurveyWebsite.Models.ViewModels
{
    public class SearchSurveyViewModel
    {
        public string? Keyword { get; set; }
        public string? SortOrder { get; set; } // "date_desc", "date_asc", "participants_desc", "participants_asc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6; // mỗi trang 6 khảo sát

        public List<Survey> Surveys { get; set; } = new();
        public int TotalPages { get; set; }
    }
}
