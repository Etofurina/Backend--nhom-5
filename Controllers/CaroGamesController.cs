using Web_API.data;
using CaroGameAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CaroGameAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CaroGamesController : ControllerBase
	{
		private readonly AppDbContext _context;

		public CaroGamesController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/CaroGames
		[HttpGet]
		public async Task<ActionResult<IEnumerable<CaroGame>>> GetCaroGames()
		{
			return await _context.CaroGames.ToListAsync();
		}

		// GET: api/CaroGames/5
		[HttpGet("{id}")]
		public async Task<ActionResult<CaroGame>> GetCaroGame(int id)
		{
			var game = await _context.CaroGames.FindAsync(id);
			if (game == null) return NotFound();
			return game;
		}

		// POST: api/CaroGames
		[HttpPost]
		public async Task<ActionResult<CaroGame>> PostCaroGame(CaroGame game)
		{
			_context.CaroGames.Add(game);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetCaroGame), new { id = game.GameId }, game);
		}

		// PUT: api/CaroGames/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutCaroGame(int id, CaroGame game)
		{
			if (id != game.GameId) return BadRequest();
			game.UpdatedAt = DateTime.Now;
			_context.Entry(game).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.CaroGames.Any(e => e.GameId == id))
					return NotFound();
				else
					throw;
			}

			return NoContent();
		}

		// DELETE: api/CaroGames/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCaroGame(int id)
		{
			var game = await _context.CaroGames.FindAsync(id);
			if (game == null) return NotFound();
			_context.CaroGames.Remove(game);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
