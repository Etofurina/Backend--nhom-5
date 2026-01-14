namespace Web_API.Dtos
{
	public class LeaderboardItemDto
	{
		public string UserName { get; set; } = string.Empty; // Tên hiển thị (FullName hoặc Email)
		public int Score { get; set; }
		public string TimePlayed { get; set; } = string.Empty; // Ví dụ: "12/01/2025"
		public int Difficulty { get; set; }
		public string TimeElapsed { get; set; }
	}
}