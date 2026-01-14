using System.ComponentModel.DataAnnotations;

namespace WebAPI2.Models
{
	public class Player
	{
		public int Id { get; set; }

		[Required]
		public string UserName { get; set; }

		public string Email { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
