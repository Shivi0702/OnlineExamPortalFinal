namespace OnlineExamPortalFinal.DTOs
{
    public class UserExamAttemptDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public bool Passed { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}