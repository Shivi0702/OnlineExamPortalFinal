namespace OnlineExamPortalFinal.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;

        public string CorrectAnswer { get; set; } = string.Empty;

        // Foreign Key
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
    }
}
