using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Web_API.data;
using Web_API.Models;

namespace Web_API.Hubs
{
	[Authorize] // Yêu cầu đăng nhập mới được chat
	public class ChatHub : Hub
	{
		// Inject DbContext để lưu dữ liệu
		private readonly AppDbContext _context;

		public ChatHub(AppDbContext context)
		{
			_context = context;
		}
		// 1. Khi User kết nối, tự động gán họ vào "nhóm riêng" trùng tên Email của họ
		// Để sau này ai muốn chat riêng thì gửi vào nhóm tên Email đó.
		public override async Task OnConnectedAsync()
		{
			var email = Context.User.FindFirstValue(ClaimTypes.Name);
			if (!string.IsNullOrEmpty(email))
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, email);
			}
			await base.OnConnectedAsync();
		}

		// 2. Chat Thế Giới (Gửi cho tất cả)
		public async Task SendMessageToGlobal(string message)
		{
			var user = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Ẩn danh";
			var email = Context.User.FindFirst(ClaimTypes.Email)?.Value ?? ""; // <--- THÊM DÒNG NÀY
			var time = DateTime.Now.ToString("HH:mm");

			// Gửi thêm biến 'email' xuống client (tổng cộng 4 tham số)
			await Clients.All.SendAsync("ReceiveGlobalMessage", user, message, time, email);
		}

		public async Task SendPrivateMessage(string targetEmail, string message)
		{
			var senderEmail = Context.User.FindFirst(ClaimTypes.Email)?.Value;
			var time = DateTime.Now;

			// 1. LƯU VÀO DATABASE
			var chatLog = new ChatMessage
			{
				SenderEmail = senderEmail,
				ReceiverEmail = targetEmail,
				Message = message,
				Timestamp = time
			};
			_context.ChatMessages.Add(chatLog);
			await _context.SaveChangesAsync(); // Lưu xong mới gửi

			// 2. GỬI SIGNALR (Như cũ)
			await Clients.Group(targetEmail).SendAsync("ReceivePrivateMessage", senderEmail, message, time.ToString("HH:mm"));
		}
	}
}
