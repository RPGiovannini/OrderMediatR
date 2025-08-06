namespace OrderMediatR.Common.Interfaces;

public interface IHttpStatusCodeReturnable
{
    int StatusCode { get; }
    string ErrorDescription { get; }
}