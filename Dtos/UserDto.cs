using System.ComponentModel.DataAnnotations;

namespace Web_API.Dtos
{
	public class UserDto
	{
		[Required]
		[EmailAddress] // <-- Tự động kiểm tra phải là định dạng email
		public string Email { get; set; } = string.Empty;

		[Required]
		[MinLength(6)]
		public string Password { get; set; } = string.Empty;

		public string FullName { get; set; } = string.Empty;

		
	}
}
