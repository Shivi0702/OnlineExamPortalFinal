using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using OnlineExamPortalFinal.Models;

namespace OnlineExamPortal.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/exam
        [HttpPost]
        public IActionResult CreateExam(CreateExamDto dto)
        {
            var exam = new Exam
            {
                Title = dto.Title,
                Description = dto.Description,
                Duration = dto.Duration,
                TotalMarks = dto.TotalMarks
            };

            _context.Exams.Add(exam);
            _context.SaveChanges();

            return Ok(new { message = "Exam created successfully", exam.ExamId });
        }

        // GET: api/exam
        [HttpGet]
        public IActionResult GetAllExams()
        {
            var exams = _context.Exams.Select(e => new ExamDetailDto
            {
                ExamId = e.ExamId,
                Title = e.Title,
                Description = e.Description,
                Duration = e.Duration,
                TotalMarks = e.TotalMarks
            }).ToList();

            return Ok(exams);
        }

        // GET: api/exam/{id}
        [HttpGet("{id}")]
        public IActionResult GetExam(int id)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null)
                return NotFound("Exam not found.");

            var dto = new ExamDetailDto
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                Description = exam.Description,
                Duration = exam.Duration,
                TotalMarks = exam.TotalMarks
            };

            return Ok(dto);
        }

        // PUT: api/exam/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateExam(int id, CreateExamDto dto)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null)
                return NotFound("Exam not found.");

            exam.Title = dto.Title;
            exam.Description = dto.Description;
            exam.Duration = dto.Duration;
            exam.TotalMarks = dto.TotalMarks;

            _context.SaveChanges();
            return Ok("Exam updated successfully.");
        }

        // DELETE: api/exam/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null)
                return NotFound("Exam not found.");

            _context.Exams.Remove(exam);
            _context.SaveChanges();
            return Ok("Exam deleted.");
        }
    }
}
