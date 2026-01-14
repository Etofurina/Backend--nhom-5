namespace Web_API.Helpers
{
	public static class RubikHelper
	{
		private static readonly string[] Moves = { "R", "L", "U", "D", "F", "B" };
		private static readonly string[] Modifiers = { "", "'", "2" }; // Xoay thường, ngược, 2 vòng

		public static string GenerateScramble(int difficulty)
		{
			// Độ khó: 1(Dễ: 8-10), 2(TB: 15-18), 3(Khó: 20-25)
			int min = difficulty == 1 ? 8 : (difficulty == 2 ? 15 : 20);
			int max = difficulty == 1 ? 10 : (difficulty == 2 ? 18 : 25);

			int length = new Random().Next(min, max + 1);
			List<string> scramble = new List<string>();
			string lastMove = "";

			Random rand = new Random();

			for (int i = 0; i < length; i++)
			{
				string move;
				// Logic: Không lặp lại nước đi vừa xong (VD: Không R rồi lại R)
				do
				{
					move = Moves[rand.Next(Moves.Length)];
				} while (move == lastMove);

				string modifier = Modifiers[rand.Next(Modifiers.Length)];
				scramble.Add(move + modifier);
				lastMove = move;
			}

			return string.Join(" ", scramble);
		}

		// Hàm tạo mã thách đấu ngẫu nhiên (VD: X8K-9LP)
		public static string GenerateChallengeCode()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, 6)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}