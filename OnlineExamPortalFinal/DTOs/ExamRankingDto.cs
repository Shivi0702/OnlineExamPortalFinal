namespace OnlineExamPortalFinal.DTOs
{
    public class ExamRankingDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int Rank { get; set; }
        public int TotalParticipants { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public int TopperScore { get; set; }
    }
}