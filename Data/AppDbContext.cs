namespace Web_API.data
{
    using System.Collections.Generic;
    using CaroGameAPI.Models;
    using Microsoft.EntityFrameworkCore;
    using Web_API.Models;
    using WebAPI2.Models;

	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<SudokuMatch> SudokuMatches { get; set; }
		public DbSet<ChatMessage> ChatMessages { get; set; }
		public DbSet<RubikGame> RubikGames { get; set; }

		public DbSet<Player> Players { get; set; }
		public DbSet<GameHistory> GameHistories { get; set; }

		public DbSet<CaroGame> CaroGames { get; set; }

		public DbSet<MatchLog> MatchLogs { get; set; }

		public DbSet<CaroMatch> CaroMatches { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<CaroGame>().ToTable("CaroGames");
		}

	}
}
