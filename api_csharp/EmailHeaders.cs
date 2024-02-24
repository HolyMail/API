using MailKit;

namespace api_csharp;

public record EmailHeaders(
    uint UniqueId,
    string From,
    string To,
    string Subject,
    DateTime DateTime,
    string Folder,
    bool IsReply,
    string PreviewText
) {
    public static EmailHeaders FromSummary(IMessageSummary summary) => new EmailHeaders(
        UniqueId: summary.UniqueId.Id,
        From: summary.Envelope.From.Mailboxes.FirstOrDefault()?.Address ?? "",
        To: summary.Envelope.To.Mailboxes.FirstOrDefault()?.Address ?? "",
        Subject: summary.Envelope.Subject,
        DateTime: summary.Date.DateTime,
        Folder: summary.Folder.Name,
        IsReply: summary.IsReply,
        PreviewText: summary.PreviewText
    );
}