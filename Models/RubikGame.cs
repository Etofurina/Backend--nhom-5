using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
	public class RubikGame
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		[ForeignKey("UserId")]
		public User User { get; set; }

		public int Mode { get; set; } // 0: Thường, 1: Tạo thách đấu, 2: Nhận thách đấu
		public int Difficulty { get; set; } // 1, 2, 3

		public string Scramble { get; set; } = string.Empty; // Chuỗi xáo trộn

		// Các trường update sau khi chơi xong
		public double? Duration { get; set; } // Thời gian giải (giây)
		public int? Mistakes { get; set; }
		public int? Score { get; set; }
			
		public DateTime PlayedAt { get; set; } = DateTime.Now;

		// Chức năng thách đấu
		public string? ChallengeCode { get; set; } // Mã chia sẻ (VD: RUBIK-888)
		public int? ParentMatchId { get; set; } // ID ván gốc (nếu đang nhận lời thách đấu)
	}
}