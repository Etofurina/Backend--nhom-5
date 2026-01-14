using Web_API.data;
using WebAPI2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI2.Models;

namespace WebAPI2.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PlayersController : ControllerBase
	{
		private readonly AppDbContext _context;
		public PlayersController(AppDbContext context) => _context = context;

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Player>>> GetPlayers() =>
			await _context.Players.ToListAsync();

		[HttpGet("{id}")]
		public async Task<ActionResult<Player>> GetPlayer(int id)
		{
			var player = await _context.Players.FindAsync(id);
			return player == null ? NotFound() : player;
		}

		[HttpPost]
		public async Task<ActionResult<Player>> CreatePlayer(Player player)
		{
			_context.Players.Add(player);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdatePlayer(int id, Player player)
		{
			if (id != player.Id) return BadRequest();
			_context.Entry(player).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePlayer(int id)
		{
			var player = await _context.Players.FindAsync(id);
			if (player == null) return NotFound();
			_context.Players.Remove(player);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
