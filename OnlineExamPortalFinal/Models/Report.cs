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

        public double Percentage { get; set; }

        public bool IsPassed { get; set; }           // <-- Add this
        public DateTime Timestamp { get; set; }      // <-- Add this

        // Navigation
        public User User { get; set; } = null!;
        public Exam Exam { get; set; } = null!;
    }
}