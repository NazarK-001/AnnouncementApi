using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnnouncementApi.Data;
using AnnouncementApi.Models;

namespace AnnouncementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly AnnouncementDbContext _context;

        public AnnouncementsController(AnnouncementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnnouncementModel>>> GetAll()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.DateAdded)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var target = await _context.Announcements.FindAsync(id);
            if (target == null)
                return NotFound();

            var targetWords = ExtractWords(target.Title + " " + target.Description);

            var similarAnnouncements = await _context.Announcements
                .Where(a => a.Id != id)
                .ToListAsync();

            var top3Similar = similarAnnouncements
                .Where(a =>
                {
                    var otherWords = ExtractWords(a.Title + " " + a.Description);
                    return targetWords.Overlaps(otherWords);
                })
                .OrderByDescending(a => a.DateAdded)
                .Take(3)
                .ToList();

            return Ok(new
            {
                target.Id,
                target.Title,
                target.Description,
                target.DateAdded,
                SimilarAnnouncements = top3Similar
            });
        }

        [HttpPost]
        public async Task<ActionResult<AnnouncementModel>> Create(AnnouncementModel model)
        {
            model.DateAdded = DateTime.UtcNow;
            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AnnouncementModel model)
        {
            if (id != model.Id) return BadRequest();

            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static HashSet<string> ExtractWords(string input)
        {
            return input
                .ToLower()
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '-', '_', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToHashSet();
        }

    }
}
