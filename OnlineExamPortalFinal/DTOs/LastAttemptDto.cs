namespace OnlineExamPortalFinal.DTOs
{
    public class LastAttemptDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}