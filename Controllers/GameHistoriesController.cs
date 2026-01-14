using Web_API.data;
using WebAPI2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI2.Models;

namespace CaroApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GameHistoriesController : ControllerBase
	{
		private readonly AppDbContext _context;
		public GameHistoriesController(AppDbContext context) => _context = context;

		[HttpGet]
		public async Task<ActionResult<IEnumerable<GameHistory>>> GetHistories() =>
			await _context.GameHistories.Include(g => g.Player).ToListAsync();

		[HttpGet("{id}")]
		public async Task<ActionResult<GameHistory>> GetHistory(int id)
		{
			var history = await _context.GameHistories.Include(g => g.Player)
													  .FirstOrDefaultAsync(g => g.Id == id);
			return history == null ? NotFound() : history;
		}

		[HttpPost]
		public async Task<ActionResult<GameHistory>> CreateHistory(GameHistory history)
		{
			_context.GameHistories.Add(history);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetHistory), new { id = history.Id }, history);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateHistory(int id, GameHistory history)
		{
			if (id != history.Id) return BadRequest();
			_context.Entry(history).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteHistory(int id)
		{
			var history = await _context.GameHistories.FindAsync(id);
			if (history == null) return NotFound();
			_context.GameHistories.Remove(history);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
