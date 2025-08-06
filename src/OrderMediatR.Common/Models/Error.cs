namespace OrderMediatR.Common.Models;

    public class Error
    {
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Detail { get; set; }
    }

