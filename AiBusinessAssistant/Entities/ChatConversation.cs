using System.ComponentModel.DataAnnotations;

namespace AiBusinessAssistant.Entities
{
    public class ChatConversation
    {
        [Key]
        public int ConversationId { get; set; }

        public string? Title { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
