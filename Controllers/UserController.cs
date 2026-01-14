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
	// 4. Tạo User mới (Admin)
	[HttpPost]
	public async Task<IActionResult> CreateUser(CreateUserDto dto)
	{
		// Check trùng email
		if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
			return BadRequest("Email đã tồn tại.");

		// Hash password (ví dụ đơn giản – nên dùng BCrypt)
		var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

		var user = new User
		{
			Email = dto.Email,
			PasswordHash = passwordHash,
			FullName = dto.FullName,
			Role = "User"
		};

		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		return Ok(new
		{
			user.Id,
			user.Email,
			user.FullName,
			user.Role
		});
	}
	// --- PHẦN B: THỐNG KÊ CÁ NHÂN ---

	
}
