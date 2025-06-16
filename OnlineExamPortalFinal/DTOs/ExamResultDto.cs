namespace OnlineExamPortalFinal.DTOs
{
    public class ExamResultDto
    {
        public int TotalMarks { get; set; }
        public int MarksObtained { get; set; }
        public double Percentage { get; set; }      
        public string ResultStatus { get; set; }
    }
}