﻿using Microsoft.AspNetCore.Authorization;
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
            bool alreadyAttempted = _context.Reports.Any(r => r.ExamId == examId && r.UserId == userId );
            if (alreadyAttempted)
                return BadRequest("You have already attempted this exam and cannot attempt it again.");

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

            var exam = _context.Exams.Find(dto.ExamId);
            if (exam == null)
                return NotFound("Exam not found.");

            int correctCount = 0;

            // 🟡 New list to collect per-question feedback
            var questionFeedback = new List<QuestionFeedbackDto>();

            foreach (var answer in dto.Answers)
            {
                var question = _context.Questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                if (question == null) continue;

                bool isCorrect = answer.Answer.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                int marks = isCorrect ? 1 : 0;
                if (isCorrect) correctCount++;

                // Save student response
                var response = new Response
                {
                    ExamId = dto.ExamId,
                    UserId = userId,
                    QuestionId = question.QuestionId,
                    Answer = answer.Answer,
                    MarksObtained = marks
                };

                _context.Responses.Add(response);

                // 🟢 Add question feedback to return in result
                questionFeedback.Add(new QuestionFeedbackDto
                {
                    QuestionText = question.Text,
                    SelectedAnswer = answer.Answer,
                    CorrectAnswer = question.CorrectAnswer
                });
            }

            // Save overall report
            var report = new Report
            {
                ExamId = dto.ExamId,
                UserId = userId,
                TotalMarks = exam.TotalMarks,
                PerformanceMetrics = $"{correctCount}/{exam.TotalMarks}",
                Percentage=((double)correctCount/exam.TotalMarks)*100
            };

            _context.Reports.Add(report);
            _context.SaveChanges();

            // 🟢 Return enhanced result DTO with per-question feedback
            return Ok(new ExamResultDto
            {
                TotalMarks = exam.TotalMarks,
                MarksObtained = correctCount,
                ResultStatus = correctCount >= exam.TotalMarks / 2 ? "Pass" : "Fail",
                Questions = questionFeedback
            });
        }
    }
}
