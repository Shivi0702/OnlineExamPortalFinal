namespace OnlineExamPortalFinal.DTOs
{
    public class UserReportDto
    {
        public int ReportId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public double Percentage { get; set; }
        public string PerformanceMetrics { get; set; }

    }
}
