namespace Web_API.Dtos
{
	public class CaroFinishDto
	{
		public int Result { get; set; } // 1: Thắng, -1: Thua
		public int Moves { get; set; }
		public double Duration { get; set; }
		public string Mode { get; set; } // "PvE"
	}
}