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
	[Authorize]
	public class RubikController : ControllerBase
	{
		private readonly AppDbContext _context;

		public RubikController(AppDbContext context)
		{
			_context = context;
		}

		// 1. START GAME
		[HttpPost("start")]
		public async Task<IActionResult> StartGame([FromBody] RubikStartDto request)
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			var game = new RubikGame
			{
				UserId = user.Id,
				Difficulty = request.Difficulty,
				PlayedAt = DateTime.Now
			};

			// Logic Thách Đấu
			if (!string.IsNullOrEmpty(request.ChallengeCode))
			{
				// Tìm ván gốc có mã này
				var parentGame = await _context.RubikGames
					.FirstOrDefaultAsync(g => g.ChallengeCode == request.ChallengeCode);

				if (parentGame == null) return BadRequest("Mã thách đấu không tồn tại!");

				// Copy cấu hình của ván gốc
				game.Scramble = parentGame.Scramble; // Quan trọng: Phải cùng đề
				game.ParentMatchId = parentGame.Id;
				game.Difficulty = parentGame.Difficulty;
				game.Mode = 2; // Chế độ: Nhận thách đấu
			}
			else
			{
				// Chơi thường -> Tạo Scramble mới
				game.Scramble = RubikHelper.GenerateScramble(request.Difficulty);
				game.Mode = 0; // Chế độ: Thường
			}

			_context.RubikGames.Add(game);
			await _context.SaveChangesAsync();

			// Trả về Scramble và TargetTime (nếu là thách đấu) để frontend hiện "Đối thủ đã giải trong 30s"
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

		// 2. FINISH GAME
		[HttpPost("finish")]
		public async Task<IActionResult> FinishGame([FromBody] RubikFinishDto request)
		{
			var game = await _context.RubikGames.FindAsync(request.MatchId);
			if (game == null) return NotFound("Game not found");

			// Cập nhật kết quả
			game.Duration = request.Duration;
			game.Mistakes = request.Mistakes;

			// Tính điểm: (Ví dụ: Khó * 1000 - Thời gian * 10 - Lỗi * 50)
			int baseScore = game.Difficulty * 1000;
			game.Score = (int)Math.Max(0, baseScore - (request.Duration * 10) - (request.Mistakes * 50));

			string message = "Hoàn thành!";
			bool isWin = false;

			// Xử lý logic Thách đấu
			if (game.Mode == 2 && game.ParentMatchId != null) // Đang nhận kèo
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
			else if (request.CreateChallenge) // Muốn tạo kèo mới
			{
				game.Mode = 1; // Chuyển thành chế độ "Tạo thách đấu"
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

		// 3. LEADERBOARD
		[HttpGet("leaderboard")]
		public async Task<IActionResult> GetLeaderboard([FromQuery] int difficulty)
		{
			var query = _context.RubikGames
				.Include(g => g.User)
				.Where(g => g.Duration != null); // Chỉ lấy ván đã xong

			if (difficulty > 0)
				query = query.Where(g => g.Difficulty == difficulty);

			var leaderboard = await query
				.OrderByDescending(g => g.Score) // Xếp theo điểm (hoặc Duration nếu anh muốn)
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

		// 4. HISTORY
		[HttpGet("history")]
		public async Task<IActionResult> GetHistory()
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
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
					Result = g.Mode == 2 ? (g.Score > 0 ? "Xong" : "Thua") : "Xong" // Logic hiển thị tạm
				})
				.ToListAsync();

			return Ok(history);
		}
	}
}