namespace api_csharp.DTO;

// ReSharper disable ClassNeverInstantiated.Global
public sealed record EmailRequestBodyDto(
    // ReSharper restore ClassNeverInstantiated.Global
    string User, 
    string Credentials, 
    int EmailsCount, 
    int Offset,
    int EmailId);