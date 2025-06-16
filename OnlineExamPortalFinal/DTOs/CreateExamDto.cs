namespace OnlineExamPortalFinal.DTOs
{
    public class CreateExamDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } // In minutes
        public int TotalMarks { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }

        public bool IsActive { get; set; }
    }
}
