using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using System.Security.Claims;
namespace OnlineExamPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/report/my
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public IActionResult GetMyReports()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var reports = _context.Reports
                .Include(r => r.Exam)
                .Where(r => r.UserId == userId)
                .Select(r => new UserReportDto
                {
                    ReportId = r.ReportId,
                    ExamId = r.ExamId,
                    ExamTitle = r.Exam.Title,
                    TotalMarks = r.TotalMarks,
                    PerformanceMetrics = r.PerformanceMetrics
                })
                .ToList();

            return Ok(reports);
        }

        // GET: api/report/exam/{examId}
        [HttpGet("exam/{examId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult GetReportsByExam(int examId)
        {
            var reports = _context.Reports
                .Include(r => r.User)
                .Include(r => r.Exam)
                .Where(r => r.ExamId == examId)
                .Select(r => new ReportDto
                {
                    ReportId = r.ReportId,
                    ExamTitle = r.Exam.Title,
                    StudentName = r.User.Name,
                    TotalMarks = r.TotalMarks,
                    PerformanceMetrics = r.PerformanceMetrics
                })
                .ToList();

            return Ok(reports);
        }

        // GET: api/report/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllReports()
        {
            var reports = _context.Reports
                .Include(r => r.Exam)
                .Include(r => r.User)
                .Select(r => new ReportDto
                {
                    ReportId = r.ReportId,
                    ExamTitle = r.Exam.Title,
                    StudentName = r.User.Name,
                    TotalMarks = r.TotalMarks,
                    PerformanceMetrics = r.PerformanceMetrics
                })
                .ToList();

            return Ok(reports);
        }
    }
}