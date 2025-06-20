using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using OnlineExamPortalFinal.Models;

namespace OnlineExamPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public IActionResult CreateExam(CreateExamDto dto)
        {
            int categoryIdToUse = 0;

            // Use CategoryId if provided and valid (greater than 0)
            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
                var category = _context.Categories.FirstOrDefault(c => c.CategoryId == dto.CategoryId.Value);
                if (category == null)
                    return BadRequest("Provided CategoryId does not exist.");
                categoryIdToUse = category.CategoryId;
            }
            // If CategoryId not valid, use CategoryName if provided
            else if (!string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                var category = _context.Categories.FirstOrDefault(c => c.Name == dto.CategoryName.Trim());
                if (category != null)
                {
                    categoryIdToUse = category.CategoryId;
                }
                else
                {
                    var newCategory = new Category { Name = dto.CategoryName.Trim() };
                    _context.Categories.Add(newCategory);
                    _context.SaveChanges();
                    categoryIdToUse = newCategory.CategoryId;
                }
            }
            else
            {
                return BadRequest("Please provide either a valid CategoryId or a CategoryName.");
            }

            var exam = new Exam
            {
                Title = dto.Title,
                Description = dto.Description,
                Duration = dto.Duration,
                TotalMarks = dto.TotalMarks,
                CategoryId = categoryIdToUse,
                //IsActive = dto.IsActive // <-- Added
            };

            _context.Exams.Add(exam);
            _context.SaveChanges();

            return Ok(new { message = "Exam created successfully!" });
        }

        [HttpGet("exams-by-category")]
        [AllowAnonymous]
        public IActionResult GetExamsGroupedByCategory()
        {
            var categoriesWithExams = _context.Categories
                .Select(cat => new {
                    CategoryId = cat.CategoryId,
                    CategoryName = cat.Name,
                    Exams = _context.Exams
                        .Where(e => e.CategoryId == cat.CategoryId)
                        .Select(e => new {
                            ExamId = e.ExamId,
                            Title = e.Title,
                            Description = e.Description,
                            Duration = e.Duration,
                            TotalMarks = e.TotalMarks,
                            //IsActive = e.IsActive // <-- Added
                        }).ToList()
                }).Where(c => c.Exams.Any())
                .ToList();

            return Ok(categoriesWithExams);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllExams()
        {
            var exams = _context.Exams.Select(e => new ExamDetailDto
            {
                ExamId = e.ExamId,
                Title = e.Title,
                Description = e.Description,
                Duration = e.Duration,
                TotalMarks = e.TotalMarks,
            }).ToList();

            return Ok(exams);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Teacher, Student, Admin")]
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
                TotalMarks = exam.TotalMarks,
            };

            return Ok(dto);
        }

        [HttpGet("by-category/{categoryId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult GetExamsByCategory(int categoryId)
        {
            var exams = _context.Exams
                .Where(e => e.CategoryId == categoryId)
                .Select(e => new { 
                    ExamId = e.ExamId, 
                    Title = e.Title }).ToList();

            return Ok(exams);

        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null)
                return NotFound("Exam not found.");

            _context.Exams.Remove(exam);
            _context.SaveChanges();
            return Ok("Exam deleted.");
        }

        [HttpGet("{examId}/leaderboard")]
        [Authorize(Roles = "Teacher, Student, Admin")]
        public async Task<IActionResult> GetLeaderboard(int examId)
        {
            var results = await _context.Responses
                .Where(r => r.ExamId == examId)
                .GroupBy(r => r.UserId)
                .Select(g => new {
                    StudentId = g.Key,
                    TotalScore = g.Sum(x => x.MarksObtained)
                })
                .OrderByDescending(x => x.TotalScore)
                .ToListAsync();

            var userIds = results.Select(r => r.StudentId).ToList();
            var users = _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u.Name);

            var leaderboard = new List<LeaderboardEntryDto>();
            int lastScore = -1;
            int rank = 0;
            int skip = 1;

            foreach (var result in results)
            {
                if (result.TotalScore != lastScore)
                {
                    rank += skip;
                    skip = 1;
                }
                else
                {
                    skip++;
                }
                lastScore = result.TotalScore;

                leaderboard.Add(new LeaderboardEntryDto
                {
                    Rank = rank,
                    StudentId = result.StudentId,
                    StudentName = users.ContainsKey(result.StudentId) ? users[result.StudentId] : "Unknown",
                    TotalScore = result.TotalScore
                });
            }

            return Ok(leaderboard);
        }
    }
}