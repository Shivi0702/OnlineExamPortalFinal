using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace OnlineExamPortalFinal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("dashboard-metrics")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.Contains("nameidentifier"))?.Value; 
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) 
                return Unauthorized();

            var allExams = await _context.Exams.ToListAsync();

            var reports = await _context.Reports
                .Include(r => r.Exam)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var allExamsList = allExams.Select(e => new AllExamDto
            {
                ExamId = e.ExamId,
                ExamName = e.Title,
                TotalMarks = e.TotalMarks,
                Duration = e.Duration + " min"
            }).ToList();

            var attemptsList = reports.Select(r => new UserExamAttemptDto
            {
                ExamId = r.ExamId,
                ExamName = r.Exam.Title,
                Score = int.TryParse(r.PerformanceMetrics.Split('/')[0], out int score) ? score : 0,
                TotalMarks = r.TotalMarks,
                Percentage = r.TotalMarks > 0 ? Math.Round((double)(int.TryParse(r.PerformanceMetrics.Split('/')[0], out int s) ? s : 0) / r.TotalMarks * 100, 2) : 0,
                Passed = r.PerformanceMetrics.StartsWith("0/") ? false : (r.TotalMarks > 0 ? ((double)(int.TryParse(r.PerformanceMetrics.Split('/')[0], out int sc) ? sc : 0) / r.TotalMarks) * 100 >= 50 : false)
            }).ToList();

            var uniqueAttemptedExamIds = attemptsList.Select(a => a.ExamId).Distinct().ToList();

            var passedExamIds = attemptsList.Where(a => a.Passed).Select(a => a.ExamId).Distinct().ToHashSet();
            var failedExamIds = attemptsList
                .Where(a => !a.Passed && !passedExamIds.Contains(a.ExamId))
                .Select(a => a.ExamId)
                .Distinct();

            var bestScoreExamGroup = reports
                .GroupBy(r => r.ExamId)
                .Select(g => new
                {
                    ExamName = g.First().Exam.Title,
                    Score = g.Sum(x => int.TryParse(x.PerformanceMetrics.Split('/')[0], out int s) ? s : 0),
                    TotalMarks = g.First().TotalMarks,
                    ExamId = g.Key
                })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            BestScoreExamDto? bestExamDto = null;
            if (bestScoreExamGroup != null && bestScoreExamGroup.TotalMarks > 0)
            {
                bestExamDto = new BestScoreExamDto
                {
                    Name = bestScoreExamGroup.ExamName,
                    Score = bestScoreExamGroup.Score,
                    TotalMarks = bestScoreExamGroup.TotalMarks,
                    Percentage = Math.Round((double)bestScoreExamGroup.Score / bestScoreExamGroup.TotalMarks * 100, 2)
                };
            }

            var examRankings = new List<ExamRankingDto>();
            foreach (var examId in uniqueAttemptedExamIds)
            {
                var allReports = await _context.Reports
                    .Include(r => r.Exam)
                    .Where(r => r.ExamId == examId)
                    .ToListAsync();

                var userScore = allReports
                    .Where(r => r.UserId == userId)
                    .Sum(r => int.TryParse(r.PerformanceMetrics.Split('/')[0], out int m) ? m : 0);

                var examTotalMarks = allReports.FirstOrDefault()?.TotalMarks ?? 0;
                var percentage = examTotalMarks > 0 ? Math.Round((double)userScore / examTotalMarks * 100, 2) : 0;

                var userScores = allReports
                    .GroupBy(r => r.UserId)
                    .Select(g => new { UserId = g.Key, Score = g.Sum(x => int.TryParse(x.PerformanceMetrics.Split('/')[0], out int s) ? s : 0) })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                var rank = userScores.FindIndex(x => x.UserId == userId) + 1;
                var topperScore = userScores.FirstOrDefault()?.Score ?? 0;
                var examName = allReports.FirstOrDefault()?.Exam?.Title ?? "Unknown Exam";

                examRankings.Add(new ExamRankingDto
                {
                    ExamId = examId,
                    ExamName = examName,
                    Rank = rank,
                    TotalParticipants = userScores.Count,
                    Score = userScore,
                    TotalMarks = examTotalMarks,
                    Percentage = percentage,
                    TopperScore = topperScore
                });
            }

            var metrics = new DashboardMetricsDto
            {
                TotalExams = allExamsList.Count,
                Attempted = uniqueAttemptedExamIds.Count,
                Passed = passedExamIds.Count,
                Failed = failedExamIds.Count(),
                BestScoreExam = bestExamDto,
                Rankings = examRankings,
                AllExams = allExamsList,
                Attempts = attemptsList
            };

            return Ok(metrics);

        }


        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); 
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId); 
            if (user == null) 
                return NotFound();

            return Ok(new
            {
                user.Name,
                user.Email,
                user.ProfileImageUrl
            });

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()

        {
            var users = _context.Users.Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email,
                u.Role,
                u.ProfileImageUrl
            }).ToList();
            return Ok(users);

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var responses = _context.Responses.Where(r => r.UserId == id);
                _context.Responses.RemoveRange(responses);
                await _context.SaveChangesAsync();

                // Remove user
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Student,Teacher,Admin")]
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfileImageUrl = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Photo uploaded successfully", url = user.ProfileImageUrl });
        }

     
        [Authorize(Roles = "Student,Teacher,Admin")]
        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            // Use BCrypt to verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.SaveChanges();

            return Ok("Password changed successfully.");
        }

    }
}