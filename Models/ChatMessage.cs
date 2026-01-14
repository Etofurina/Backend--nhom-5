// File: Models/ChatMessage.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
	public class ChatMessage
	{
		[Key]
		public int Id { get; set; }
		public string SenderEmail { get; set; }
		public string ReceiverEmail { get; set; }
		public string Message { get; set; }
		public DateTime Timestamp { get; set; }
	}
}