namespace OnlineExamPortalFinal.DTOs
{
    public class UserReportDto
    {
        public int ReportId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int TotalMarks { get; set; }
        public string PerformanceMetrics { get; set; }
        public bool IsPassed { get; set; }
    }
}
