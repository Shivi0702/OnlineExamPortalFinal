namespace OnlineExamPortalFinal.DTOs
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int TotalScore { get; set; }
    }
}