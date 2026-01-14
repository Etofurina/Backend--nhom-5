namespace Web_API.Dtos
{
	public class UserStatsDto
	{
		public int TotalMatches { get; set; } // Tổng số ván chơi
		public int Wins { get; set; }         // Số ván thắng
		public int Losses { get; set; }       // Số ván thua/bỏ cuộc
		public double WinRate { get; set; }   // Tỷ lệ thắng (%)
		public int TotalScore { get; set; }   // Tổng điểm tích lũy
		public string RankTitle { get; set; } = "Tân binh"; // Danh hiệu
	}
}