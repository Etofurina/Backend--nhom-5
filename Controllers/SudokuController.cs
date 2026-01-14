using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API.data;
using Web_API.Dtos;
using Web_API.Helpers;
using Web_API.Models;
using WebAPI2.Dtos; // Đảm bảo namespace chứa LeaderboardItemDto

namespace Web_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SudokuController : ControllerBase
	{
		private readonly AppDbContext _context;

		public SudokuController(AppDbContext context)
		{
			_context = context;
		}

		// ============================================================
		// 1. START GAME
		// ============================================================
		[HttpPost("start")]
		public async Task<IActionResult> StartGame([FromBody] StartGameDto request)
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			if (request.Difficulty < 1 || request.Difficulty > 3)
				return BadRequest("Độ khó không hợp lệ.");

			// 1. Tạo đề bài (Thực tế nên random)
			string solution = "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

			// 2. Tạo bàn cờ đục lỗ
			string gameBoard = SudokuLogic.GenerateBoardFromSolution(solution, request.Difficulty);

			// 3. Khởi tạo điểm số tối đa ban đầu
			int initialScore = request.Difficulty * 1000;

			var match = new SudokuMatch
			{
				UserId = user.Id,
				CurrentBoard = gameBoard,
				SolutionBoard = solution,
				Difficulty = request.Difficulty,
				Score = initialScore,
				MistakeCount = 0,
				HintCount = 0, // [MỚI] Khởi tạo số lần gợi ý bằng 0
				CreatedAt = DateTime.Now,
				IsCompleted = false
			};

			_context.SudokuMatches.Add(match);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				Message = "Game started!",
				GameId = match.Id,
				Board = match.CurrentBoard,
				Difficulty = match.Difficulty,
				Score = match.Score,
				HintCount = match.HintCount
			});
		}

		// ============================================================
		// 2. GET GAME (Load lại game cũ)
		// ============================================================
		[HttpGet("{id}")]
		public async Task<IActionResult> GetGame(int id)
		{
			var match = await _context.SudokuMatches.FindAsync(id);
			if (match == null) return NotFound("Không tìm thấy ván game.");

			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (match.UserId != user.Id) return Forbid();

			// Tính toán lại điểm hiện tại dựa trên thời gian trôi qua
			UpdateMatchScore(match);
			// Lưu ý: Có thể không cần SaveChanges ở đây nếu chỉ muốn hiển thị, 
			// nhưng để đồng bộ nhất thì nên lưu.

			return Ok(new
			{
				GameId = match.Id,
				Board = match.CurrentBoard,
				Score = match.Score,
				MistakeCount = match.MistakeCount,
				HintCount = match.HintCount, // Trả về số lần gợi ý
				IsCompleted = match.IsCompleted
			});
		}

		// ============================================================
		// 3. MAKE MOVE (Đi một nước)
		// ============================================================
		[HttpPost("move")]
		public async Task<IActionResult> MakeMove([FromBody] SudokuMoveDto move)
		{
			var match = await _context.SudokuMatches.FindAsync(move.MatchId);
			if (match == null) return NotFound("Game not found.");

			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (match.UserId != user.Id) return Forbid();

			if (match.IsCompleted) return BadRequest("Game đã kết thúc.");

			// Logic kiểm tra đúng sai
			int index = move.Row * 9 + move.Col;
			char correctChar = match.SolutionBoard[index];
			char inputChar = char.Parse(move.Value.ToString());
			bool isCorrect = (correctChar == inputChar);

			if (isCorrect)
			{
				// Cập nhật bàn cờ
				match.CurrentBoard = SudokuLogic.UpdateBoardString(match.CurrentBoard, move.Row, move.Col, move.Value);

				// Kiểm tra hoàn thành
				if (match.CurrentBoard == match.SolutionBoard)
				{
					match.IsCompleted = true;
				}
			}
			else
			{
				// Tăng lỗi
				match.MistakeCount += 1;
			}

			// [QUAN TRỌNG] Cập nhật điểm số theo công thức chuẩn
			UpdateMatchScore(match);

			await _context.SaveChangesAsync();

			return Ok(new
			{
				Message = isCorrect ? (match.IsCompleted ? "WIN" : "Good move") : "Wrong move",
				IsCorrect = isCorrect,
				Board = match.CurrentBoard,
				Score = match.Score,
				MistakeCount = match.MistakeCount,
				HintCount = match.HintCount, // Trả về để frontend đồng bộ
				IsCompleted = match.IsCompleted
			});
		}

		// ============================================================
		// 4. [MỚI] GET HINT (Lấy gợi ý)
		// ============================================================
		[HttpPost("hint/{id}")]
		public async Task<IActionResult> GetHint(int id)
		{
			var match = await _context.SudokuMatches.FindAsync(id);
			if (match == null) return NotFound("Game not found.");

			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (match.UserId != user.Id) return Forbid();

			if (match.IsCompleted) return BadRequest("Game đã kết thúc.");

			// 1. Kiểm tra giới hạn
			if (match.HintCount >= 3)
			{
				return BadRequest(new { Message = "Bạn đã hết 3 lần gợi ý." });
			}

			// 2. Tìm ô trống đầu tiên
			int emptyIndex = match.CurrentBoard.IndexOf('0');
			if (emptyIndex == -1) return BadRequest("Bàn cờ đã đầy.");

			// 3. Lấy giá trị đúng
			char correctValueChar = match.SolutionBoard[emptyIndex];
			int correctValue = int.Parse(correctValueChar.ToString());
			int row = emptyIndex / 9;
			int col = emptyIndex % 9;

			// 4. Điền vào bàn cờ
			StringBuilder sb = new StringBuilder(match.CurrentBoard);
			sb[emptyIndex] = correctValueChar;
			match.CurrentBoard = sb.ToString();

			// 5. Tăng số lần gợi ý
			match.HintCount++;

			// 6. Tính lại điểm (Sẽ bị trừ điểm do HintCount tăng và thời gian trôi qua)
			UpdateMatchScore(match);

			// 7. Kiểm tra thắng
			if (match.CurrentBoard == match.SolutionBoard)
			{
				match.IsCompleted = true;
			}

			await _context.SaveChangesAsync();

			return Ok(new
			{
				Message = $"Đã gợi ý ô ({row + 1}, {col + 1}).",
				Board = match.CurrentBoard,
				Score = match.Score,
				HintCount = match.HintCount,
				SuggestedRow = row,
				SuggestedCol = col,
				SuggestedValue = correctValue,
				IsCompleted = match.IsCompleted
			});
		}
		// API lấy thống kê cá nhân (Sửa lỗi null ở màn hình Hồ sơ)
		[HttpGet("stats")]
		[Authorize]
		public async Task<IActionResult> GetMyStats()
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			// 1. Lấy tất cả các ván đấu của user này
			var matches = await _context.SudokuMatches
				.Where(m => m.UserId == user.Id)
				.ToListAsync();

			// 2. Tính toán các chỉ số
			int totalGames = matches.Count;
			int wins = matches.Count(m => m.IsCompleted);
			int totalScore = matches.Sum(m => m.Score);

			double winRate = 0;
			if (totalGames > 0)
			{
				winRate = (double)wins / totalGames * 100;
			}

			// 3. Logic danh hiệu (Rank Title)
			string rankTitle = "Tân thủ";
			if (totalScore > 5000) rankTitle = "Tập sự";
			if (totalScore > 20000) rankTitle = "Cao thủ";
			if (totalScore > 50000) rankTitle = "Đại kiện tướng";
			if (totalScore > 100000) rankTitle = "Thần bài Sudoku";

			// 4. Trả về JSON
			return Ok(new
			{
				FullName = string.IsNullOrEmpty(user.FullName) ? user.Email : user.FullName,
				TotalGamesPlayed = totalGames,
				GamesWon = wins,
				WinRate = Math.Round(winRate, 1), // Làm tròn 1 số thập phân
				TotalScoreAccumulated = totalScore,
				RankTitle = rankTitle
			});
		}

		// ============================================================
		// 5. HELPER: Hàm tính điểm chung
		// ============================================================
		private void UpdateMatchScore(SudokuMatch match)
		{
			// 1. Tính thời gian đã chơi (giây)
			var timeSpan = DateTime.Now - match.CreatedAt;
			int totalSeconds = (int)timeSpan.TotalSeconds;

			// 2. Điểm gốc
			int baseScore = match.Difficulty * 1000;

			// 3. Các loại phạt
			int mistakePenalty = match.MistakeCount * 50; // 50 điểm/lỗi
			int hintPenalty = match.HintCount * 100;      // 100 điểm/gợi ý
			int timePenalty = totalSeconds * 1;           // 1 điểm/giây

			// 4. Tổng kết
			int finalScore = baseScore - mistakePenalty - hintPenalty - timePenalty;

			// 5. Gán vào match (không âm)
			match.Score = Math.Max(0, finalScore);
		}

		// ============================================================
		// 6. HISTORY
		// ============================================================
		[HttpGet("history")]
		public async Task<ActionResult> GetMyHistory()
		{
			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return Unauthorized();

			var history = await _context.SudokuMatches
				.Where(m => m.UserId == user.Id)
				.OrderByDescending(m => m.CreatedAt)
				.Select(m => new
				{
					m.Id,
					Status = m.IsCompleted ? "Hoàn thành" : "Đang chơi",
					Difficulty = m.Difficulty,
					Score = m.Score,
					MistakeCount = m.MistakeCount,
					HintCount = m.HintCount, // Trả về lịch sử dùng gợi ý
					Date = m.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
					TimeElapsed = (int)(DateTime.Now - m.CreatedAt).TotalMinutes
				})
				.ToListAsync();

			return Ok(history);
		}

		// ============================================================
		// 7. SURRENDER
		// ============================================================
		[HttpPut("surrender/{id}")]
		public async Task<IActionResult> SurrenderGame(int id)
		{
			var match = await _context.SudokuMatches.FindAsync(id);
			if (match == null) return NotFound("Game not found.");

			var email = User.FindFirstValue(ClaimTypes.Name);
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (match.UserId != user.Id) return Forbid();

			if (match.IsCompleted) return BadRequest("Game đã kết thúc.");

			match.IsCompleted = true;
			match.CurrentBoard = match.SolutionBoard;
			match.Score = 0; // Đầu hàng về 0 điểm

			await _context.SaveChangesAsync();

			return Ok(new { Message = "Đã đầu hàng.", Board = match.SolutionBoard });
		}

		// ============================================================
		// 8. DELETE
		// ============================================================
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteGame(int id)
		{
			var match = await _context.SudokuMatches.FindAsync(id);
			if (match == null) return NotFound();

			var email = User.FindFirstValue(ClaimTypes.Name);
			// Logic admin/user như cũ
			if (!User.IsInRole("Admin"))
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
				if (match.UserId != user.Id) return Forbid();
			}

			_context.SudokuMatches.Remove(match);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "Deleted successfully." });
		}

		// ============================================================
		// 9. LEADERBOARD (Đã bổ sung TimePlayed - Ngày chơi)
		// ============================================================
		[HttpGet("leaderboard")]
		public async Task<ActionResult<List<LeaderboardItemDto>>> GetLeaderboard([FromQuery] int difficulty)
		{
			var query = _context.SudokuMatches
				.Include(m => m.User)
				.Where(m => m.IsCompleted == true);

			if (difficulty > 0)
			{
				query = query.Where(m => m.Difficulty == difficulty);
			}

			var matches = await query
				.OrderByDescending(m => m.Score)
				.Take(10)
				.ToListAsync();

			var result = matches.Select(m =>
			{
				// 1. Tính toán thời gian chơi (Duration) từ điểm số
				int maxScore = m.Difficulty * 1000;
				int penalties = (m.MistakeCount * 50) + (m.HintCount * 100);
				int secondsPlayed = maxScore - penalties - m.Score;

				if (secondsPlayed < 0) secondsPlayed = 0;

				TimeSpan t = TimeSpan.FromSeconds(secondsPlayed);
				string timeFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

				return new LeaderboardItemDto
				{
					UserName = string.IsNullOrEmpty(m.User.FullName) ? m.User.Email : m.User.FullName,
					Score = m.Score,
					Difficulty = m.Difficulty,

					// Đây là cái bạn đang cần sửa (05:30)
					TimeElapsed = timeFormatted,

					// [BỔ SUNG] Thêm dòng này để hiện ngày chơi (12/01/2026)
					TimePlayed = m.CreatedAt.ToString("dd/MM/yyyy")
				};
			}).ToList();

			return Ok(result);
		}

	}
}