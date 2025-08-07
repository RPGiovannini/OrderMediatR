namespace OrderMediatR.SyncWorker.Messages
{
    public class ProductMessage
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "BRL";
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal Weight { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}