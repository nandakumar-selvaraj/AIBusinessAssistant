namespace AiBusinessAssistant.Common.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(string message) : base(message) { }
}

public sealed class ApiValidationException(string message) : ApiException(message);
public sealed class ApiNotFoundException(string message) : ApiException(message);
public sealed class ApiConflictException(string message) : ApiException(message);
