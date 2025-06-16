using System.Collections.Generic;

namespace OnlineExamPortalFinal.DTOs
{
    public class ExamResultDto
    {
        public int TotalMarks { get; set; }
        public int MarksObtained { get; set; }
        public string ResultStatus { get; set; } = string.Empty;

        public List<QuestionFeedbackDto> Questions { get; set; } = new();
    }
}
