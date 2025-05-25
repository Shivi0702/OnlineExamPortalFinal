namespace OnlineExamPortalFinal.DTOs
{
    public class SubmitExamDto
    {
        public int ExamId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }
}
