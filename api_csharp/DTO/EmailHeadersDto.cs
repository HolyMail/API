namespace api_csharp.DTO;

public sealed record EmailHeadersDto(bool End, EmailHeaders[] Data, int Length);