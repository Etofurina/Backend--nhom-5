using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Web_API.data;
using Web_API.Dtos;
using Web_API.Models;
using Web_API.Services;
using WebAPI2.Dtos;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;
	private readonly EmailService _emailService;

	// ✅ CHỈ 1 CONSTRUCTOR
	public AuthController(
		AppDbContext context,
		IConfiguration configuration,
		EmailService emailService)
	{
		_context = context;
		_configuration = configuration;
		_emailService = emailService;
	}

	// ================= REGISTER =================
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] UserDto request)
	{
		if (await _context.Users.AnyAsync(u => u.Email == request.Email))
			return BadRequest("Email này đã được đăng ký.");

		var user = new User
		{
			Email = request.Email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
			FullName = request.FullName,
			Role = "User"
		};

		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		return Ok("Đăng ký thành công!");
	}

	// ================= LOGIN STEP 1 =================
	[HttpPost("login-step1")]
	public async Task<IActionResult> LoginStep1([FromBody] LoginDto request)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
		if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			return BadRequest("Sai tài khoản hoặc mật khẩu.");

		string otp = new Random().Next(100000, 999999).ToString();
		user.OtpCode = otp;
		user.OtpExpiration = DateTime.Now.AddMinutes(2);

		await _context.SaveChangesAsync();

		_emailService.SendEmail(
			user.Email,
			"Xác thực đăng nhập",
			$"Mã đăng nhập của bạn: <b>{otp}</b>"
		);

		return Ok("Vui lòng kiểm tra Email để nhập OTP.");
	}

	// ================= LOGIN STEP 2 =================
	[HttpPost("login-step2")]
	public async Task<IActionResult> LoginStep2([FromBody] VerifyOtpDto request)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

		if (user == null ||
			user.OtpCode != request.OtpCode ||
			user.OtpExpiration == null ||
			user.OtpExpiration < DateTime.Now)
		{
			return BadRequest("Mã OTP sai hoặc đã hết hạn.");
		}

		user.OtpCode = null;
		user.OtpExpiration = null;
		await _context.SaveChangesAsync();

		return Ok(CreateToken(user));
	}

	// ================= FORGOT PASSWORD =================
	[HttpPost("forgot-password")]
	public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
		if (user == null)
			return BadRequest("Email không tồn tại.");

		string otp = new Random().Next(100000, 999999).ToString();
		user.OtpCode = otp;
		user.OtpExpiration = DateTime.Now.AddMinutes(5);

		await _context.SaveChangesAsync();

		_emailService.SendEmail(
			user.Email,
			"Mã OTP đặt lại mật khẩu",
			$"<h1>{otp}</h1><p>Hết hạn sau 5 phút</p>"
		);

		return Ok("Đã gửi OTP.");
	}

	// ================= RESET PASSWORD =================
	[HttpPost("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

		if (user == null ||
			user.OtpCode != request.OtpCode ||
			user.OtpExpiration == null ||
			user.OtpExpiration < DateTime.Now)
		{
			return BadRequest("OTP sai hoặc hết hạn.");
		}

		user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
		user.OtpCode = null;
		user.OtpExpiration = null;

		await _context.SaveChangesAsync();
		return Ok("Đổi mật khẩu thành công!");
	}

	// ================= TOKEN =================
	private string CreateToken(User user)
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name, user.Email),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Role, user.Role)
		};

		var key = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!)
		);

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

		var token = new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddDays(1),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
	[HttpPost("resend-otp-login")]
	public async Task<IActionResult> ResendOtpLogin([FromBody] ForgotPasswordDto request) // Tận dụng DTO có chứa Email
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
		if (user == null) return BadRequest("Email không tồn tại.");

		// Tạo OTP mới
		string otp = new Random().Next(100000, 999999).ToString();
		user.OtpCode = otp;
		user.OtpExpiration = DateTime.Now.AddMinutes(2); // Gia hạn thêm 2 phút
		await _context.SaveChangesAsync();

		// Gửi mail
		_emailService.SendEmail(user.Email, "Gửi lại mã đăng nhập", $"Mã OTP mới của bạn là: <b>{otp}</b>");

		return Ok("Đã gửi lại mã OTP.");
	}
}
