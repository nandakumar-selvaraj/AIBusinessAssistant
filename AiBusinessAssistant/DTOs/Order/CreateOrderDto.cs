namespace AiBusinessAssistant.DTOs.Order
{
    public class CreateOrderDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();
    }
}
