using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI2.Models
{
	public class GameHistory
	{
		public int Id { get; set; }

		[Required]
		[ForeignKey("Player")]
		public int PlayerId { get; set; }
		public string Opponent { get; set; } // Người hoặc Máy
		public int BoardSize { get; set; }
		public int Difficulty { get; set; } // 0-easy,1-medium,2-hard
		public string Moves { get; set; } // JSON
		public string Result { get; set; } // "Win"/"Lose"/"Draw"
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public Player Player { get; set; }
	}
}
