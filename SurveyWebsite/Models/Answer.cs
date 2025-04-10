    using System;
    using System.Collections.Generic;

    namespace SurveyWebsite.Models;

    public partial class Answer
    {
        public int AnswerId { get; set; }

        public int ParticipationId { get; set; }

        public int QuestionId { get; set; }

        public int? OptionId { get; set; }

        public string? AnswerText { get; set; }

        public virtual Option? Option { get; set; }

        public virtual Participation Participation { get; set; } = null!;

        public virtual Question Question { get; set; } = null!;
    }
