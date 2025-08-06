using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderMediatR.Common.Interfaces;
using OrderMediatR.Common.Models;

namespace OrderMediatR.Common.Exceptions;


public static class ExceptionExtensions
{
    public static string GetName(this Exception exception)
    {
        return exception
            .GetType()
            .Name
            .Replace("Exception", "")
            .Replace("`1", "");
    }

    public static AggregateException CreateAgregateException<TException>(this List<TException> exceptions) where TException : Exception
    {
        return new AggregateException(exceptions.ToArray());
    }

    public static ErrorResponse GenerateErrorResponse(this Exception primaryException, HttpContext httpContext)
    {
        var errors = primaryException
            .GenerateErros()
            .Where(e => e != null)
            .ToList();

        return new ErrorResponse
        {
            Instance = httpContext.Request.Path,
            Errors = errors
        };
    }

    public static int GetStatusCodeFromException(this Exception exception)
    {
        if (exception == default)
            return StatusCodes.Status500InternalServerError;

        if (exception is IHttpStatusCodeReturnable exceptionCodeReturnable)
            return exceptionCodeReturnable.StatusCode;

        if (exception is AggregateException aggregateException)
            return GetStatusCodeFromException(aggregateException.InnerExceptions.First());

        return exception switch
        {
            var e when e is
                ValidationException => StatusCodes.Status400BadRequest,
            DbUpdateConcurrencyException => StatusCodes.Status404NotFound,
            DbUpdateException => StatusCodes.Status403Forbidden,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    public static IEnumerable<Error> GenerateErros(this Exception primaryException)
    {
        if (primaryException is AggregateException aggregateException)
        {
            foreach (var exception in aggregateException.InnerExceptions)
                yield return GenerateError(exception);
        }
        else
            yield return GenerateError(primaryException);
    }

    private static Error GenerateError(Exception exception)
    {
        return new Error
        {
            Type = exception.GetName(),
            Description = exception is IHttpStatusCodeReturnable ex ? ex.ErrorDescription : exception.GetName(),
            Detail = exception.Message
        };
    }
}