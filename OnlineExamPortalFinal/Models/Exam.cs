namespace OnlineExamPortalFinal.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; } // in minutes
        public int TotalMarks { get; set; }

        // Add this property to enable "active" exams logic
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Question>? Questions { get; set; }

        // Foreign key for Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
