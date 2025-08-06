namespace OrderMediatR.Common.Exceptions;

public class InvalidParameterValueException : Exception
{
    //public class InvalidParameterValueException(string parameter)
    //: Exception(Translation.GetTranslatedMessage(nameof(Messages.IvalidParameterValueException_detail), Translation.GetTranslatedMessage(parameter))),
    //        IHttpStatusCodeReturnable
    //{
    //    public int StatusCode => StatusCodes.Status400BadRequest;
    //    public string ErrorDescription => nameof(Messages.IvalidParameterValueException_error);
    //}
}