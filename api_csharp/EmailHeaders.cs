using System.Net.Sockets;
using MailKit;
using MimeKit;

namespace api_csharp;

public record MailBox(string Name, string Address);

public record EmailHeaders(
    uint UniqueId,
    MailBox From,
    MailBox To,
    string Subject,
    DateTime DateTime,
    string Folder,
    bool IsReply,
    string PreviewText
) {
    public static EmailHeaders FromSummary(IMessageSummary summary)
    {
        
        return new EmailHeaders(
            UniqueId: summary.UniqueId.Id,
            From: new MailBox(
                    Name: summary.Envelope.From[0].Name,
                    Address: summary.Envelope.From.Mailboxes.FirstOrDefault()?.Address ?? ""
                ),
            To: new MailBox(
                Name: summary.Envelope.To[0].Name,
                Address: summary.Envelope.To.Mailboxes.FirstOrDefault()?.Address ?? ""
            ),
            Subject: summary.Envelope.Subject,
            DateTime: summary.Date.DateTime,
            Folder: summary.Folder.Name,
            IsReply: summary.IsReply,
            PreviewText: summary.PreviewText
        );
    }
}