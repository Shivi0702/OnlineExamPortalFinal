namespace OnlineExamPortalFinal.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public ICollection<Exam> Exams { get; set; }
    }
}