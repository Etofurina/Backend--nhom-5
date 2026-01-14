namespace Web_API.data
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Web_API.Models;

	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<SudokuMatch> SudokuMatches { get; set; }
		public DbSet<ChatMessage> ChatMessages { get; set; }
		public DbSet<RubikGame> RubikGames { get; set; }

	}
}
