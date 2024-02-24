using api_csharp;
using api_csharp.DTO;
using api_csharp.Extensions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region apple

app.MapPost("emails/apple/list", async ([FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = new ImapClient();
    try
    {
        await imapClient.ConnectAsync(EmailServerConfig.AppleImapServerAddress, 993, true);

        await imapClient.AuthenticateAsync(body.User, body.Credentials);

        var inbox = imapClient.Inbox;

        await inbox.OpenAsync(FolderAccess.ReadOnly);

        var results = await inbox.FetchAsync(body.EmailsCount, body.Offset);

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
    finally
    {
        await imapClient.DisconnectAsync(true);
    }
});

app.MapPost("emails/apple/send", async ([FromBody] EmailSendBodyDto body) =>
{
    try
    {
        using var message = body.ToMimeMessage();

        var smtpClient = new SmtpClient();
    
        await smtpClient.ConnectAsync(EmailServerConfig.AppleSmtpServerAddress, 587, SecureSocketOptions.StartTls);
    
        await smtpClient.AuthenticateAsync(body.User, body.Credentials);

        await smtpClient.SendAsync(message);
        
        return Results.Ok(new
        {
            Message = "Email successfully sent",
        });
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
});

app.MapPost("emails/apple/details", async ([FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = new ImapClient();
    try
    {
        await imapClient.ConnectAsync(EmailServerConfig.AppleImapServerAddress, 993, true);

        await imapClient.AuthenticateAsync(body.User, body.Credentials);

        var inbox = imapClient.Inbox;

        await inbox.OpenAsync(FolderAccess.ReadOnly);

        var message = await inbox.GetMessageAsync(body.EmailId);

        var results = EmailDetailsDto.FromMimeMessage(message);

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
    finally
    {
        await imapClient.DisconnectAsync(true);
    }
});

#endregion

#region google

app.MapPost("emails/google/list", async ([FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = new ImapClient();
    try
    {
        await imapClient.ConnectAsync(EmailServerConfig.GoogleImapServerAddress, 993, true);
    
        await imapClient.AuthenticateAsync(new SaslMechanismOAuth2(body.User, body.Credentials));

        var inbox = imapClient.Inbox;
        
        await inbox.OpenAsync(FolderAccess.ReadOnly);
        
        var results = await inbox.FetchAsync(body.EmailsCount, body.Offset);

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
    finally
    {
        await imapClient.DisconnectAsync(true);
    }
});

app.MapPost("emails/google/send", async ([FromBody] EmailSendBodyDto body) =>
{
    try
    {
        var message = body.ToMimeMessage();

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(EmailServerConfig.GoogleSmtpServerAddress, 587, SecureSocketOptions.StartTls);

        await smtpClient.AuthenticateAsync(new SaslMechanismOAuth2(body.User, body.Credentials));

        await smtpClient.SendAsync(message);

        return Results.Ok(new
        {
            Message = "Email successfully sent",
        });
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
});

app.MapPost("emails/google/details", async ([FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = new ImapClient();
    try
    {
        await imapClient.ConnectAsync(EmailServerConfig.GoogleImapServerAddress, 993, true);
    
        await imapClient.AuthenticateAsync(new SaslMechanismOAuth2(body.User, body.Credentials));

        var inbox = imapClient.Inbox;

        await inbox.OpenAsync(FolderAccess.ReadOnly);

        var message = await inbox.GetMessageAsync(body.EmailId);

        var results = EmailDetailsDto.FromMimeMessage(message);

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
    finally
    {
        await imapClient.DisconnectAsync(true);
    }
});


#endregion

app.Run();