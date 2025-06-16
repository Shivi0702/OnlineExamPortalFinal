using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using OnlineExamPortalFinal.Models;
using System.Security.Claims;
using System.Linq;

namespace OnlineExamPortalFinal.Controllers
{
    [Authorize(Roles = "Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class ExamAttemptController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExamAttemptController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("start/{examId}")]
        public IActionResult StartExam(int examId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Restrict re-attempt if already passed
            bool alreadyPassed = _context.Reports.Any(r => r.ExamId == examId && r.UserId == userId && r.IsPassed);
            if (alreadyPassed)
                return BadRequest("You have already passed this exam and cannot attempt it again.");

            var exam = _context.Exams.Find(examId);
            if (exam == null)
                return NotFound("Exam not found.");

            var questions = _context.Questions
                .Where(q => q.ExamId == examId)
                .Select(q => new ExamQuestionDto
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    Option1 = q.Option1,
                    Option2 = q.Option2,
                    Option3 = q.Option3,
                    Option4 = q.Option4
                }).ToList();

            return Ok(new
            {
                ExamId = examId,
                ExamTitle = exam.Title,
                Duration = exam.Duration,
                Questions = questions
            });
        }

        [HttpPost("submit")]
        public IActionResult SubmitExam(SubmitExamDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            bool alreadyPassed = _context.Reports.Any(r => r.ExamId == dto.ExamId && r.UserId == userId && r.IsPassed);
            if (alreadyPassed)
                return BadRequest("You have already passed this exam and cannot attempt it again.");

            var exam = _context.Exams.Find(dto.ExamId);
            if (exam == null)
                return NotFound("Exam not found.");

            var questions = _context.Questions.Where(q => q.ExamId == dto.ExamId).ToList();
            var totalQuestions = questions.Count;
            int marksObtained = 0;

            var responses = new List<Response>();

            foreach (var answer in dto.Answers)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                if (question == null) continue;

                bool isCorrect = answer.Answer.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                int marks = isCorrect ? 1 : 0; // Change as per marks per question if needed
                if (isCorrect) marksObtained++;

                var response = new Response
                {
                    ExamId = dto.ExamId,
                    UserId = userId,
                    QuestionId = question.QuestionId,
                    Answer = answer.Answer,
                    MarksObtained = marks,
                    Timestamp = DateTime.UtcNow
                };
                responses.Add(response);
            }

            // Correct percentage calculation
            double percentage = totalQuestions > 0 ? ((double)marksObtained / totalQuestions) * 100 : 0;
            int passPercentage = 40; // Pass threshold
            bool isPassed = percentage >= passPercentage;

            foreach (var response in responses)
                response.IsPassed = isPassed;

            _context.Responses.AddRange(responses);

            // Separate out score and percentage in PerformanceMetrics or add columns
            var report = new Report
            {
                ExamId = dto.ExamId,
                UserId = userId,
                TotalMarks = totalQuestions, // Total questions attempted or possible
                PerformanceMetrics = $"{marksObtained}/{totalQuestions}", // Only score, no percent sign
                Percentage = Math.Round(percentage, 2), // <-- Add this column in DB/model if not present
                IsPassed = isPassed,
                Timestamp = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            _context.SaveChanges();

            return Ok(new ExamResultDto
            {
                TotalMarks = totalQuestions,
                MarksObtained = marksObtained,
                Percentage = Math.Round(percentage, 2),
                ResultStatus = isPassed ? "Pass" : "Fail"
            });
        }
    }
}