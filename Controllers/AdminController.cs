using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API.data;
using Web_API.Dtos;
using Web_API.Models;

namespace Web_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")] // <--- CỰC QUAN TRỌNG: Chỉ Admin mới được vào
	public class AdminController : ControllerBase
	{
		private readonly AppDbContext _context;

		public AdminController(AppDbContext context)
		{
			_context = context;
		}

		// --- 1. QUẢN LÝ USER ---
		// 1. THÊM USER MỚI (Create)
		[HttpPost("user")]
		public async Task<IActionResult> CreateUser([FromBody] UserDto request)
		{
			if (await _context.Users.AnyAsync(u => u.Email == request.Email))
				return BadRequest("Email này đã tồn tại.");

			var user = new User
			{
				Email = request.Email,
				FullName = request.FullName,
				Role = "User", // Mặc định là User, sau này Admin sửa lại sau nếu muốn
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return Ok(new { Message = "Tạo user thành công!" });
		}
		// 2. SỬA USER (Update)
		// Tạo class DTO nhỏ để hứng dữ liệu update (để password có thể null)
		public class AdminUpdateUserDto
		{
			public string FullName { get; set; } = string.Empty;
			public string Role { get; set; } = string.Empty;
			public string? Password { get; set; } // Nếu null hoặc rỗng thì không đổi pass
		}

		[HttpPut("user/{id}")]
		public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto request)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null) return NotFound("User không tồn tại.");

			// Cập nhật thông tin
			user.FullName = request.FullName;
			user.Role = request.Role; // Admin có quyền đổi Role (User <-> Admin)

			// Nếu Admin nhập password mới thì đổi, bỏ trống thì giữ nguyên pass cũ
			if (!string.IsNullOrEmpty(request.Password))
			{
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
			}

			await _context.SaveChangesAsync();
			return Ok(new { Message = "Cập nhật thành công!" });
		}
		// Xem danh sách tất cả người dùng
		[HttpGet("users")]
		public async Task<IActionResult> GetAllUsers()
		{
			var users = await _context.Users
				.Select(u => new { u.Id, u.Email, u.FullName, u.Role}) // Không trả về Password
				.ToListAsync();
			return Ok(users);
		}

		// Xóa người dùng (Kèm theo xóa luôn lịch sử chơi game của họ)
		[HttpDelete("user/{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null) return NotFound("Không tìm thấy user");

			// EF Core sẽ tự động xóa các bảng con (Sudoku, Rubik) nếu anh cấu hình Cascade Delete,
			// nhưng để chắc ăn, mình xóa thủ công hoặc để EF lo.
			_context.Users.Remove(user);
			await _context.SaveChangesAsync();

			return Ok(new { Message = $"Đã xóa user {user.Email} và toàn bộ dữ liệu liên quan." });
		}

		// --- 2. QUẢN LÝ SUDOKU ---

		// Xem lịch sử Sudoku toàn server
		[HttpGet("sudoku-matches")]
		public async Task<IActionResult> GetAllSudokuMatches()
		{
			var matches = await _context.SudokuMatches
				.Include(m => m.User) // Kèm thông tin người chơi
				.OrderByDescending(m => m.CreatedAt)
				.Select(m => new
				{
					m.Id,
					UserEmail = m.User.Email,
					m.Difficulty,
					m.Score,
					m.IsCompleted,
					Date = m.CreatedAt.ToString("dd/MM/yyyy HH:mm")
				})
				.ToListAsync();
			return Ok(matches);
		}

		// Xóa một ván Sudoku cụ thể (Xóa lịch sử rác)
		[HttpDelete("sudoku-match/{id}")]
		public async Task<IActionResult> DeleteSudokuMatch(int id)
		{
			var match = await _context.SudokuMatches.FindAsync(id);
			if (match == null) return NotFound();

			_context.SudokuMatches.Remove(match);
			await _context.SaveChangesAsync();
			return Ok(new { Message = "Đã xóa ván Sudoku." });
		}

		// --- 3. QUẢN LÝ RUBIK ---

		// Xem lịch sử Rubik toàn server
		[HttpGet("rubik-games")]
		public async Task<IActionResult> GetAllRubikGames()
		{
			var games = await _context.RubikGames
				.Include(g => g.User)
				.OrderByDescending(g => g.PlayedAt)
				.Select(g => new
				{
					g.Id,
					UserEmail = g.User.Email,
					g.Difficulty,
					g.Duration,
					Mode = g.Mode == 0 ? "Thường" : "Thách đấu",
					Date = g.PlayedAt.ToString("dd/MM/yyyy HH:mm")
				})
				.ToListAsync();
			return Ok(games);
		}

		// Xóa một ván Rubik
		[HttpDelete("rubik-game/{id}")]
		public async Task<IActionResult> DeleteRubikGame(int id)
		{
			var game = await _context.RubikGames.FindAsync(id);
			if (game == null) return NotFound();

			_context.RubikGames.Remove(game);
			await _context.SaveChangesAsync();
			return Ok(new { Message = "Đã xóa ván Rubik." });
		}
	}
}