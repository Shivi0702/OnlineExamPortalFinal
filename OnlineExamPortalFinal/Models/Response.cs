namespace OnlineExamPortalFinal.Models
{
    public class Response
    {
        public int ResponseId { get; set; }
        public int ExamId { get; set; }
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public string Answer { get; set; } = string.Empty;
        public int MarksObtained { get; set; }
        public bool IsPassed { get; set; }

        // Add this! Set this in your submission logic.
        public DateTime Timestamp { get; set; }

        public Exam Exam { get; set; } = null!;
        public User User { get; set; } = null!;
        public Question Question { get; set; } = null!;
    }
}