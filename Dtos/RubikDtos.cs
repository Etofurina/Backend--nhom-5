namespace Web_API.Dtos
{
	public class RubikStartDto
	{
		public int Difficulty { get; set; } // 1, 2, 3
		public string? ChallengeCode { get; set; } // Nếu có thì là đang nhận thách đấu
	}

	public class RubikFinishDto
	{
		public int MatchId { get; set; }
		public double Duration { get; set; }
		public int Mistakes { get; set; }
		public bool CreateChallenge { get; set; } // User có muốn tạo mã thách đấu từ ván này không?
	}
}