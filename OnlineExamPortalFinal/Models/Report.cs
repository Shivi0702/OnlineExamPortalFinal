namespace OnlineExamPortalFinal.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public int ExamId { get; set; }
        public int UserId { get; set; }
        public int TotalMarks { get; set; }
        public string PerformanceMetrics { get; set; } = string.Empty;
        //public DateTime AttemptedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Exam Exam { get; set; } = null!;
    }
}
