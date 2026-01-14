namespace Web_API.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Email { get; set; } = string.Empty; 
		public string PasswordHash { get; set; } = string.Empty;
		public string Role { get; set; } = "User";
		public string FullName { get; set; } = string.Empty;

		public string? OtpCode { get; set; } // Lưu mã OTP (VD: 123456)
		public DateTime? OtpExpiration { get; set; } // Thời gian hết hạn (VD: sau 5 phút)
	}
}
