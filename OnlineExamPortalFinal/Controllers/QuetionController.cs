using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using OnlineExamPortalFinal.Models;
namespace OnlineExamPortal.Controllers
{
    [Authorize(Roles = "Teacher,Student")]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public QuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddQuestion(CreateQuestionDto dto)
        {
            var exam = _context.Exams.Find(dto.ExamId);
            if (exam == null)
                return NotFound("Exam not found.");

            var question = new Question
            {
                ExamId = dto.ExamId,
                Text = dto.Text,
                Option1 = dto.Option1,
                Option2 = dto.Option2,
                Option3 = dto.Option3,
                Option4 = dto.Option4,
                CorrectAnswer = dto.CorrectAnswer
            };

            _context.Questions.Add(question);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetQuestionsByExam), new { examId = question.ExamId }, new { message = "Question added", questionId = question.QuestionId });
        }


        [HttpGet("exam/{examId}")]
        public IActionResult GetQuestionsByExam(int examId)
        {
            var questions = _context.Questions
                .Where(q => q.ExamId == examId)
                .Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,

                    Option1 = q.Option1,
                    Option2 = q.Option2,
                    Option3 = q.Option3,
                    Option4 = q.Option4,
                    CorrectAnswer = q.CorrectAnswer
                }).ToList();

            return Ok(questions);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateQuestion(int id, CreateQuestionDto dto)
        {
            var question = _context.Questions.Find(id);
            if (question == null)
                return NotFound(new { message = "Question not found" });

            question.Text = dto.Text;
            question.Option1 = dto.Option1;
            question.Option2 = dto.Option2;
            question.Option3 = dto.Option3;
            question.Option4 = dto.Option4;
            question.CorrectAnswer = dto.CorrectAnswer;

            _context.SaveChanges();

            return Ok(new { message = "Question updated successfully", questionId = question.QuestionId });
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null)
                return NotFound(new { message = "Question not found" });

            _context.Questions.Remove(question);
            _context.SaveChanges();

            return Ok(new { message = "Question deleted successfully", questionId = id });
        }

    }
}
