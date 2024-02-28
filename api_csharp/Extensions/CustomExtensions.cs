using api_csharp.Cache;
using api_csharp.DTO;
using MailKit;

namespace api_csharp.Extensions;

public static class CustomExtensions
{
    public static async Task<EmailHeadersDto> FetchAsync(this IMailFolder inbox, int emailsCount, int offset)
    {
        var max = emailsCount == -1 ? inbox.Count - 1 : inbox.Count - offset - 1;
        var min = inbox.Count - offset - emailsCount;
        
        if (max > inbox.Count - 1)
        {
            max = inbox.Count - 1;
        }

        if (min <= 0)
        {
            min = 0;
        }
        
        var results = (await inbox
                .FetchAsync(min, max,
                    MessageSummaryItems.Body | MessageSummaryItems.Envelope | MessageSummaryItems.UniqueId ))
            .Select(EmailHeaders.FromSummary)
            .OrderByDescending(x => x.DateTime)
            .ToArray();
        

        var end = min == 0;
        
        return new EmailHeadersDto(end, results, results.Length);
    }
}