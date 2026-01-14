using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_API.data;


namespace Web_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Bắt buộc phải đăng nhập mới lấy được lịch sử
	public class ChatController : ControllerBase
	{
		private readonly AppDbContext _context;

		public ChatController(AppDbContext context)
		{
			_context = context;
		}

		[HttpGet("history")]
		public async Task<IActionResult> GetChatHistory([FromQuery] string partnerEmail)
		{
			// Lấy email của người đang đăng nhập từ Token
			var myEmail = User.FindFirst(ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(myEmail)) return Unauthorized();

			// Lấy 50 tin nhắn giữa 2 người (Tôi gửi cho họ HOẶC Họ gửi cho tôi)
			var messages = await _context.ChatMessages
				.Where(m => (m.SenderEmail == myEmail && m.ReceiverEmail == partnerEmail) ||
							(m.SenderEmail == partnerEmail && m.ReceiverEmail == myEmail))
				.OrderByDescending(m => m.Timestamp)
				.Take(50)
				.Select(m => new {
					sender = m.SenderEmail,
					msg = m.Message,
					time = m.Timestamp.ToString("HH:mm")
				})
				.ToListAsync();

			return Ok(messages);
		}
	}
}