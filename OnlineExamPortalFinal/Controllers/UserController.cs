using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using System.Security.Claims;

namespace OnlineExamPortal.Controllers
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

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            var dto = new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(dto);
        }

        [HttpPut("profile")]
        public IActionResult UpdateProfile(UpdateProfileDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            user.Name = dto.Name;
            user.Email = dto.Email;

            _context.SaveChanges();

            return Ok("Profile updated.");
        }
    }
}
