using MimeKit;

namespace api_csharp.DTO;

public sealed record EmailDetailsDto(
    string From,
    string To,
    string Subject,
    DateTime DateTime,
    string Body
) {

    public static EmailDetailsDto FromMimeMessage(MimeMessage mimeMessage) => new EmailDetailsDto(
        From: mimeMessage.From.FirstOrDefault()?.Name ?? "",
        To: mimeMessage.To.FirstOrDefault()?.Name ?? "",
        Subject: mimeMessage.Subject,
        DateTime: mimeMessage.Date.DateTime,
        Body: mimeMessage.HtmlBody
    );

}
