using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortalFinal.Data;

namespace OnlineExamPortalFinal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/category

        [HttpGet("/api/categories")]
        [Authorize(Roles = "Admin,Teacher")]

        public IActionResult GetCategories()
        {
            var categories = _context.Categories
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name
                })
                .ToList();

            return Ok(categories);
        }
    }
}
