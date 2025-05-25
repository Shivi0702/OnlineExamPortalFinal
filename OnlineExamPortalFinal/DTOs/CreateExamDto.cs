namespace OnlineExamPortalFinal.DTOs
{
    public class CreateExamDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } // In minutes
        public int TotalMarks { get; set; }
    }
}
