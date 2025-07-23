namespace OrderMediatR.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommandResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
    }
}