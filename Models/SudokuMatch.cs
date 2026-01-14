using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
	public class SudokuMatch
	{
		public int Id { get; set; }
		public string CurrentBoard { get; set; } = string.Empty;
		public string SolutionBoard { get; set; } = string.Empty;
		public bool IsCompleted { get; set; } = false;
		public int UserId { get; set; }
		[ForeignKey("UserId")]
		public User User { get; set; }

		// --- CÁC TRƯỜNG MỚI BỔ SUNG ---
		public int Difficulty { get; set; } // 1: Dễ, 2: Trung bình, 3: Khó
		public int Score { get; set; } = 0; // Điểm số hiện tại
		public int MistakeCount { get; set; } = 0; // Số lần đi sai

		public int HintCount { get; set; } // Số lần đã dùng gợi ý

		public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày giờ tạo game
	}
}