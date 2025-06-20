namespace OnlineExamPortalFinal.DTOs
{
    public class DashboardMetricsDto
    {
        public int TotalExams { get; set; }
        public int Attempted { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }

        public BestScoreExamDto? BestScoreExam { get; set; }
        public List<ExamRankingDto> Rankings { get; set; } = new();
        public List<AllExamDto> AllExams { get; set; } = new();
        public List<UserExamAttemptDto> Attempts { get; set; } = new();
    }

    public class BestScoreExamDto
    {
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
    }

   
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


    public class AllExamDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int TotalMarks { get; set; }
        public string Duration { get; set; } = string.Empty;
    }

    public class UserExamAttemptDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public bool Passed { get; set; }
    }
}