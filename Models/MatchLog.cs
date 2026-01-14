using System.ComponentModel.DataAnnotations;

namespace WebAPI2.Models
{
	public class MatchLog
	{
		[Key]
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public string Winner { get; set; } = string.Empty;
		public string Mode { get; set; } = string.Empty;
		public string Difficulty { get; set; } = string.Empty;
		public int ScoreEarned { get; set; }
		public string Player1Name { get; set; } = string.Empty;
		public string Player2Name { get; set; } = string.Empty;
	}
}
