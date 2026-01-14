namespace WebAPI2.Dtos
{
	public class VerifyOtpDto
	{
		public string Email { get; set; } = string.Empty;
		public string OtpCode { get; set; } = string.Empty;
	}
}
