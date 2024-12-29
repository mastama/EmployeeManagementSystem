namespace BaseLibrary.Responses;

public record GeneralResponse(string ResponseCode, string ResponseMessage, object? Data);