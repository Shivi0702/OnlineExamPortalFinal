namespace OnlineExamPortalFinal.DTOs
{
    public class CreateQuestionDto
    {
        public int ExamId { get; set; }
        public string Text { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }

        public string CorrectAnswer { get; set; }

    }
}
