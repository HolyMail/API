using MimeKit;

namespace api_csharp.DTO;

public sealed record EmailSendBodyDto(
    string User, 
    string Credentials, 
    string FromAddress,
    string FromName,
    string ToAddress,
    string ToName,
    string Subject,
    string Message
) {
    public MimeMessage ToMimeMessage() => new()
        {
            From = { new MailboxAddress(FromName, FromAddress) },
            To = { new MailboxAddress(ToName, ToAddress) },
            Subject = Subject,
            Body = new TextPart("plain")
            {
                Text = Message,
            },
        };
}
    