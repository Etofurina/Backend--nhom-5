using System;
using System.ComponentModel.DataAnnotations;

namespace CaroGameAPI.Models
{
	public class CaroGame
	{
		[Key]
		public int GameId { get; set; }

		[Required]
		public string PlayerX { get; set; }

		[Required]
		public string PlayerO { get; set; }

		[Required]
		public string PlayerXType { get; set; } // human / ai

		[Required]
		public string PlayerOType { get; set; } // human / ai

		public int? PlayerXLevel { get; set; }
		public int? PlayerOLevel { get; set; }

		[Required]
		public string Board { get; set; } // JSON

		public string Winner { get; set; } // X, O, Draw

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
	}
}
