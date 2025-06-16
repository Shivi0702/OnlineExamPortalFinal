using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;
using OnlineExamPortalFinal.DTOs;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace OnlineExamPortal.Controllers
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
        private string HashPassword (string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            // Return the image URL as well!
            var result = new
            {
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ProfileImageUrl = user.ProfileImageUrl // <-- this line added!
            };

            return Ok(result);
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

        [Authorize(Roles = "Student")]
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

        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            // Use BCrypt to verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect.");

            // Hash the new password with BCrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.SaveChanges();

            return Ok("Password changed successfully.");
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
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            return NotFound("User not found.");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully." });
        }

    }
}
