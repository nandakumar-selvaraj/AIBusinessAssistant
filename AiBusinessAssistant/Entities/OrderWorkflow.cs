using System.ComponentModel.DataAnnotations;

namespace AiBusinessAssistant.Entities;

public sealed class OrderWorkflow
{
    [Key]
    public int ConversationId { get; set; }
    [Required]
    public string State { get; set; } = "Idle";
    public int? CustomerId { get; set; }
    public int? CreatedOrderId { get; set; }
    [Required]
    public string OrderNumber { get; set; } = string.Empty;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderWorkflowItem> Items { get; set; } = new List<OrderWorkflowItem>();
}

public sealed class OrderWorkflowItem
{
    public int ConversationId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
