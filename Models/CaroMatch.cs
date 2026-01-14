using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
	public class CaroMatch
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		[ForeignKey("UserId")]
		public User User { get; set; }

		// Kết quả: 1 = Thắng, 0 = Hòa, -1 = Thua
		public int Result { get; set; }

		// Số nước đã đi (để so sánh ai thắng nhanh hơn)
		public int Moves { get; set; }

		// Thời gian chơi (giây)
		public double Duration { get; set; }

		public DateTime PlayedAt { get; set; } = DateTime.Now;

		// Loại game: "PvE" (Đấu máy) hoặc "PvP" (Đấu người - Local)
		public string Mode { get; set; } = "PvE";
	}
}