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

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.Contains("nameidentifier"))?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            // All active exams
            var allExams = await _context.Exams.Where(e => e.IsActive).ToListAsync();

            // All user attempts (Responses)
            var attempts = await _context.Responses
                .Include(r => r.Exam)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            // AllExams list
            var allExamsList = allExams.Select(e => new AllExamDto
            {
                ExamId = e.ExamId,
                ExamName = e.Title,
                TotalMarks = e.TotalMarks,
                Duration = e.Duration + " min"
            }).ToList();

            // Attempts list
            var attemptsList = attempts.Select(a => new UserExamAttemptDto
            {
                ExamId = a.ExamId,
                ExamName = a.Exam.Title,
                Score = a.MarksObtained,
                TotalMarks = a.Exam.TotalMarks,
                Percentage = a.Exam.TotalMarks > 0 ? Math.Round((double)a.MarksObtained / a.Exam.TotalMarks * 100, 2) : 0,
                Passed = a.IsPassed,
                AttemptDate = a.Timestamp
            }).ToList();

            // Unique attempted exams
            var uniqueAttemptedExamIds = attemptsList.Select(a => a.ExamId).Distinct().ToList();

            // Passed/Failed
            var passedExamIds = attemptsList.Where(a => a.Passed).Select(a => a.ExamId).Distinct().ToHashSet();
            var failedExamIds = attemptsList
                .Where(a => !a.Passed && !passedExamIds.Contains(a.ExamId))
                .Select(a => a.ExamId)
                .Distinct();

            // --- RE-ATTEMPT EXAMS LOGIC ---
            var reAttemptExamIds = attemptsList
                .Where(a => !a.Passed)
                .Select(a => a.ExamId)
                .Distinct()
                .Except(passedExamIds) // Remove those that are eventually passed
                .ToList();

            var reAttemptExams = allExams
                .Where(e => reAttemptExamIds.Contains(e.ExamId))
                .Select(e => new AvailableExamDto
                {
                    ExamId = e.ExamId,
                    ExamName = e.Title,
                    TotalMarks = e.TotalMarks,
                    Duration = e.Duration + " min"
                }).ToList();

            // Best Score Exam
            var bestScoreExamGroup = attempts
                .GroupBy(a => a.ExamId)
                .Select(g => new
                {
                    ExamName = g.First().Exam.Title,
                    Score = g.Sum(x => x.MarksObtained),
                    TotalMarks = g.First().Exam.TotalMarks,
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

            // Last Attempt (latest by Timestamp)
            var lastAttempt = attempts.FirstOrDefault();
            LastAttemptDto? lastAttemptDto = null;
            if (lastAttempt != null)
            {
                var score = lastAttempt.MarksObtained;
                var totalMarks = lastAttempt.Exam.TotalMarks;
                var percentage = totalMarks > 0 ? Math.Round((double)score / totalMarks * 100, 2) : 0;
                lastAttemptDto = new LastAttemptDto
                {
                    Name = lastAttempt.Exam.Title,
                    Date = lastAttempt.Timestamp,
                    Score = score,
                    TotalMarks = totalMarks,
                    Percentage = percentage,
                    Result = lastAttempt.IsPassed ? "Passed" : "Failed"
                };
            }

            // Exam-wise Rankings
            var examRankings = new List<ExamRankingDto>();
            foreach (var examId in uniqueAttemptedExamIds)
            {
                var allResponses = await _context.Responses
                    .Include(r => r.Exam)
                    .Where(r => r.ExamId == examId)
                    .ToListAsync();

                var userScore = allResponses.Where(r => r.UserId == userId).Sum(r => r.MarksObtained);
                var examTotalMarks = allResponses.FirstOrDefault()?.Exam?.TotalMarks ?? 0;
                var percentage = examTotalMarks > 0 ? Math.Round((double)userScore / examTotalMarks * 100, 2) : 0;

                var userScores = allResponses
                    .GroupBy(r => r.UserId)
                    .Select(g => new { UserId = g.Key, Score = g.Sum(x => x.MarksObtained) })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                var rank = userScores.FindIndex(x => x.UserId == userId) + 1;
                var topperScore = userScores.FirstOrDefault()?.Score ?? 0;
                var examName = allResponses.FirstOrDefault()?.Exam?.Title ?? "Unknown Exam";

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

            // Available Exams (not attempted)
            var attemptedExamIdSet = new HashSet<int>(uniqueAttemptedExamIds);
            var availableExams = await _context.Exams
                .Where(e => e.IsActive && !attemptedExamIdSet.Contains(e.ExamId))
                .ToListAsync();

            var availableExamDtos = availableExams.Select(e => new AvailableExamDto
            {
                ExamId = e.ExamId,
                ExamName = e.Title,
                TotalMarks = e.TotalMarks,
                Duration = e.Duration + " min"
            }).ToList();

            var metrics = new DashboardMetricsDto
            {
                TotalExams = allExamsList.Count,
                Attempted = uniqueAttemptedExamIds.Count,
                Passed = passedExamIds.Count,
                Failed = failedExamIds.Count(),
                BestScoreExam = bestExamDto,
                LastAttempt = lastAttemptDto,
                Rankings = examRankings,
                AvailableExams = availableExamDtos,
                AllExams = allExamsList,
                Attempts = attemptsList,
                ReAttemptExams = reAttemptExams // <<---- ADD THIS
            };

            return Ok(metrics);
        }
    }
}