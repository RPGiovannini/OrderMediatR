namespace OrderMediatR.Common.Models;

public class ErrorResponse
{
    public string Instance { get; set; }
    public List<Error>? Errors { get; set; }
}
