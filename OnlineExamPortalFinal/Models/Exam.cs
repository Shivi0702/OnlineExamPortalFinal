namespace OnlineExamPortalFinal.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; } // in minutes
        public int TotalMarks { get; set; }

        public ICollection<Question>? Questions { get; set; }
    }
}
