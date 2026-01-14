using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_API.data;
using Web_API.Models;
using Microsoft.EntityFrameworkCore;
using Web_API.Dtos;
using System.Security.Claims;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // TRÙM CUỐI: Chỉ Admin mới được dùng TOÀN BỘ controller này
public class UserController : ControllerBase
{
	private readonly AppDbContext _context;

	public UserController(AppDbContext context)
	{
		_context = context;
	}

	// 1. Lấy danh sách tất cả User (Chỉ Admin thấy)
	[HttpGet]
	public async Task<ActionResult<List<User>>> GetAllUsers()
	{
		// Trả về danh sách nhưng không trả PasswordHash để bảo mật
		var users = await _context.Users
		.Select(u => new {
		u.Id,
		u.Email, 
		u.Role,
		u.FullName
	})
	.ToListAsync();

		return Ok(users);
	}

	// 2. Lấy chi tiết 1 User
	[HttpGet("{id}")]
	public async Task<ActionResult<User>> GetUserById(int id)
	{
		var user = await _context.Users.FindAsync(id);
		if (user == null) return NotFound("Không tìm thấy user.");
		return Ok(user);
	}

	// 3. Xóa User (Nguy hiểm -> Cần Admin)
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteUser(int id)
	{
		var user = await _context.Users.FindAsync(id);
		if (user == null) return NotFound("Không tìm thấy user để xóa.");

		_context.Users.Remove(user);
		await _context.SaveChangesAsync();

		return Ok($"Đã xóa user có ID: {id}");
	}
	// --- PHẦN B: THỐNG KÊ CÁ NHÂN ---

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
}
