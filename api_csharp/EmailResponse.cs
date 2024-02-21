using MailKit;

namespace api_csharp;

public sealed record EmailResponse(
    string From,
    string To,
    string Subject,
    string Message,
    DateTime DateTime
);