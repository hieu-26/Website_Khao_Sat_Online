using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyWebsite.Models;

public partial class Option
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Nội dung tùy chọn không được để trống")]
    [StringLength(200, ErrorMessage = "Tùy chọn không vượt quá 200 ký tự")]
    public string OptionText { get; set; } = null!;

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Question Question { get; set; } = null!;
}
