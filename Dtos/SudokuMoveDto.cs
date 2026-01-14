namespace Web_API.Dtos
{
	public class SudokuMoveDto
	{
		public int MatchId { get; set; }
		public int Row { get; set; } // Hàng (0-8)
		public int Col { get; set; } // Cột (0-8)
		public int Value { get; set; } // Giá trị (1-9)
	}
}