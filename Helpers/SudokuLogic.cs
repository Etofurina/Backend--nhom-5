namespace Web_API.Helpers
{
	public static class SudokuLogic
	{
		// Hàm kiểm tra nước đi có hợp lệ không
		public static bool IsValidMove(string boardStr, int row, int col, int value)
		{
			int[,] board = StringToMatrix(boardStr);

			// 1. Kiểm tra hàng ngang
			for (int c = 0; c < 9; c++)
				if (board[row, c] == value) return false;

			// 2. Kiểm tra hàng dọc
			for (int r = 0; r < 9; r++)
				if (board[r, col] == value) return false;

			// 3. Kiểm tra ô vuông 3x3
			int startRow = (row / 3) * 3;
			int startCol = (col / 3) * 3;
			for (int r = 0; r < 3; r++)
			{
				for (int c = 0; c < 3; c++)
				{
					if (board[startRow + r, startCol + c] == value) return false;
				}
			}

			return true;
		}

		// Helper: Chuyển chuỗi thành mảng 2 chiều để dễ tính toán
		public static int[,] StringToMatrix(string str)
		{
			int[,] matrix = new int[9, 9];
			for (int i = 0; i < 81; i++)
			{
				matrix[i / 9, i % 9] = int.Parse(str[i].ToString());
			}
			return matrix;
		}

		// Helper: Cập nhật chuỗi bàn cờ sau khi đi
		public static string UpdateBoardString(string currentBoard, int row, int col, int value)
		{
			char[] chars = currentBoard.ToCharArray();
			int index = row * 9 + col;
			chars[index] = char.Parse(value.ToString());
			return new string(chars);
		}
		// Hàm tạo đề bài dựa trên đáp án và độ khó
		public static string GenerateBoardFromSolution(string solution, int difficulty)
		{
			// Difficulty 1 (Dễ): Đục 30 lỗ
			// Difficulty 2 (Vừa): Đục 45 lỗ
			// Difficulty 3 (Khó): Đục 55 lỗ
			int holesToRemove = difficulty == 1 ? 30 : (difficulty == 2 ? 45 : 55);

			char[] board = solution.ToCharArray();
			Random rand = new Random();
			int attempts = holesToRemove;

			while (attempts > 0)
			{
				int index = rand.Next(0, 81);
				if (board[index] != '0') // Nếu ô đó chưa bị xóa
				{
					board[index] = '0'; // Xóa số đi (thành 0)
					attempts--;
				}
			}
			return new string(board);
		}
	}
}