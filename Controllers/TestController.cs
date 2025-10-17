using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stratoapi.Data;
using stratoapi.Models;

namespace stratoapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Test>>> GetStudents()
    {
        return await _context.Test.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Test>> GetStudent(int id)
    {
        var student = await _context.Test.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        return student;
    }

    [HttpPost]
    public async Task<ActionResult<Test>> PostStudent(Test test)
    {
        _context.Test.Add(test);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStudent), new { id = test.Id }, test);
    }
}
