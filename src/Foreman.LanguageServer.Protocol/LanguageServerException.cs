namespace Foreman.LanguageServer.Protocol;

public enum ErrorCode {
    MissingHeader,
    MalformedMessage,
    InvalidContentLength,
    MethodNotFound,
    UnableToParseMessage
}

public class LanguageServerException : Exception {
    public LanguageServerException(string message) : base(message) {}

    public static LanguageServerException FromCode(ErrorCode errorCode) {
        return errorCode switch {
            ErrorCode.MissingHeader
                => new("Message payload is missing content header."),
            ErrorCode.MalformedMessage 
                => new("Message payload was not properly delimited."), 
            ErrorCode.InvalidContentLength
                => new("Content length contains non numeric characters."),
            ErrorCode.MethodNotFound
                => new("Message method could not be found within the payload."),
            ErrorCode.UnableToParseMessage
                => new("Message contents were unable to be deserialized."),
            _ => new("An unexpected error occured.")
        };
    }
}