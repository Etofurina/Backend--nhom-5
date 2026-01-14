using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_API.data;
using Web_API.Dtos;
using Web_API.Helpers;
using Web_API.Models;

namespace Web_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // 🔒 Mặc định: cần đăng nhập
	public class RubikController : ControllerBase
	{
		private readonly AppDbContext _context;

		public RubikController(AppDbContext context)
		{
			_context = context;
		}

		// =========================
		// 1. START GAME (AUTH)
		// =========================
		[HttpPost("start")]
		public async Task<IActionResult> StartGame([FromBody] RubikStartDto request)
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			if (email == null) return Unauthorized();

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			var game = new RubikGame
			{
				UserId = user.Id,
				Difficulty = request.Difficulty,
				PlayedAt = DateTime.Now
			};

			// Thách đấu
			if (!string.IsNullOrEmpty(request.ChallengeCode))
			{
				var parentGame = await _context.RubikGames
					.FirstOrDefaultAsync(g => g.ChallengeCode == request.ChallengeCode);

				if (parentGame == null)
					return BadRequest("Mã thách đấu không tồn tại!");

				game.Scramble = parentGame.Scramble;
				game.ParentMatchId = parentGame.Id;
				game.Difficulty = parentGame.Difficulty;
				game.Mode = 2; // Nhận kèo
			}
			else
			{
				game.Scramble = RubikHelper.GenerateScramble(request.Difficulty);
				game.Mode = 0; // Chơi thường
			}

			_context.RubikGames.Add(game);
			await _context.SaveChangesAsync();

			double? targetTime = null;
			if (game.ParentMatchId != null)
			{
				var parent = await _context.RubikGames.FindAsync(game.ParentMatchId);
				targetTime = parent?.Duration;
			}

			return Ok(new
			{
				MatchId = game.Id,
				Scramble = game.Scramble,
				TargetTime = targetTime
			});
		}

		// =========================
		// 2. FINISH GAME (AUTH)
		// =========================
		[HttpPost("finish")]
		public async Task<IActionResult> FinishGame([FromBody] RubikFinishDto request)
		{
			var game = await _context.RubikGames.FindAsync(request.MatchId);
			if (game == null) return NotFound("Game not found");

			game.Duration = request.Duration;
			game.Mistakes = request.Mistakes;

			int baseScore = game.Difficulty * 1000;
			game.Score = (int)Math.Max(
				0,
				baseScore - (request.Duration * 10) - (request.Mistakes * 50)
			);

			string message = "Hoàn thành!";
			bool isWin = false;

			if (game.Mode == 2 && game.ParentMatchId != null)
			{
				var parentGame = await _context.RubikGames.FindAsync(game.ParentMatchId);
				if (parentGame != null && game.Duration < parentGame.Duration)
				{
					isWin = true;
					message = "Bạn đã thắng người thách đấu!";
				}
				else
				{
					message = "Bạn đã thua!";
				}
			}
			else if (request.CreateChallenge)
			{
				game.Mode = 1;
				game.ChallengeCode = RubikHelper.GenerateChallengeCode();
				message = "Đã tạo mã thách đấu thành công.";
			}

			await _context.SaveChangesAsync();

			return Ok(new
			{
				Score = game.Score,
				ChallengeCode = game.ChallengeCode,
				IsWin = isWin,
				Message = message
			});
		}

		// =========================
		// 3. LEADERBOARD (PUBLIC)
		// =========================
		
		[HttpGet("leaderboard")]
		public async Task<IActionResult> GetLeaderboard([FromQuery] int difficulty)
		{
			var query = _context.RubikGames
				.Include(g => g.User)
				.Where(g => g.Duration != null);

			if (difficulty > 0)
				query = query.Where(g => g.Difficulty == difficulty);

			var leaderboard = await query
				.OrderByDescending(g => g.Score)
				.Take(10)
				.Select(g => new
				{
					UserName = g.User.FullName ?? g.User.Email,
					Score = g.Score,
					Time = g.Duration,
					Date = g.PlayedAt.ToString("dd/MM/yyyy")
				})
				.ToListAsync();

			return Ok(leaderboard);
		}

		// =========================
		// 4. HISTORY (AUTH)
		// =========================
		[HttpGet("history")]
		public async Task<IActionResult> GetHistory()
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			if (email == null) return Unauthorized();

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			var history = await _context.RubikGames
				.Where(g => g.UserId == user.Id && g.Duration != null)
				.OrderByDescending(g => g.PlayedAt)
				.Select(g => new
				{
					g.Id,
					g.Difficulty,
					g.Score,
					Time = g.Duration,
					Mode = g.Mode == 0 ? "Thường" : (g.Mode == 1 ? "Tạo Kèo" : "Nhận Kèo"),
					Result = g.Mode == 2
						? (g.Score > 0 ? "Thắng" : "Thua")
						: "Xong"
				})
				.ToListAsync();

			return Ok(history);
		}
	}
}
