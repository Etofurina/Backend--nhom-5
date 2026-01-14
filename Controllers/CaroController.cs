using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_API.data;
using Web_API.Dtos;
using Web_API.Models;

namespace Web_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CaroController : ControllerBase
	{
		private readonly AppDbContext _context;

		public CaroController(AppDbContext context)
		{
			_context = context;
		}

		// 1. Lưu kết quả trận đấu
		[HttpPost("finish")]
		public async Task<IActionResult> FinishMatch([FromBody] CaroFinishDto request)
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			var match = new CaroMatch
			{
				UserId = user.Id,
				Result = request.Result,
				Moves = request.Moves,
				Duration = request.Duration,
				Mode = request.Mode,
				PlayedAt = DateTime.Now
			};

			_context.CaroMatches.Add(match);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "Đã lưu kết quả Caro!" });
		}

		// 2. Lịch sử đấu
		[HttpGet("history")]
		public async Task<IActionResult> GetHistory()
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

			var history = await _context.CaroMatches
				.Where(m => m.UserId == user.Id)
				.OrderByDescending(m => m.PlayedAt)
				.Select(m => new
				{
					Result = m.Result == 1 ? "Thắng" : (m.Result == -1 ? "Thua" : "Hòa"),
					m.Moves,
					m.Duration,
					Date = m.PlayedAt.ToString("dd/MM/yyyy HH:mm")
				})
				.ToListAsync();

			return Ok(history);
		}

		// 3. Bảng xếp hạng (Chỉ tính số trận Thắng PvE)
		[HttpGet("leaderboard")]
		public async Task<IActionResult> GetLeaderboard()
		{
			var leaderboard = await _context.CaroMatches
				.Where(m => m.Result == 1 && m.Mode == "PvE") // Chỉ lấy trận thắng với máy
				.GroupBy(m => m.UserId)
				.Select(g => new
				{
					UserId = g.Key,
					Wins = g.Count(), // Tổng số trận thắng
					BestTime = g.Min(x => x.Duration) // Trận thắng nhanh nhất
				})
				.OrderByDescending(x => x.Wins)
				.Take(10)
				.Join(_context.Users, stat => stat.UserId, user => user.Id, (stat, user) => new
				{
					UserName = user.FullName ?? user.Email,
					Wins = stat.Wins,
					BestTime = stat.BestTime
				})
				.ToListAsync();

			return Ok(leaderboard);
		}
	}
}