using api_csharp;
using api_csharp.Cache;
using api_csharp.DTO;
using api_csharp.Extensions;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// CACHE
builder.Services
    .AddSingleton(Cache.Create<string, ImapClient>(100_000))
    .AddSingleton(Cache.Create<string, SmtpClient>(50_000));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

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

// CORS
app.UseCors("AllowAll");

// CACHE
app.UseResponseCaching();


#region apple

app.MapPost("emails/apple/list", async (
    Cache<string, ImapClient> cache, 
    [FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = cache.Get(body.User);
    try
    {
        if (imapClient is null)
        {
            imapClient = new ImapClient();
            
            var removed = cache.Add(body.User, imapClient);

            if (removed is not null)
            {
                await removed.DisconnectAsync(true);
            }
        }
        
        var temporaryConnectionCreated = !imapClient.IsIdle;
        if (!imapClient.IsIdle)
        {
            imapClient = new ImapClient();
        }
        
        if (!imapClient.IsConnected)
        {
            await imapClient.ConnectAsync(EmailServerConfig.AppleImapServerAddress, 993, true);

            await imapClient.AuthenticateAsync(body.User, body.Credentials);
        }

        var inbox = imapClient.Inbox;
        
        if (!inbox.IsOpen)
        {
            await inbox.OpenAsync(FolderAccess.ReadOnly);
        }

        var results = await inbox.FetchAsync(body.EmailsCount, body.Offset);

        if (temporaryConnectionCreated)
        {
            await imapClient.DisconnectAsync(true);
        }

        return Results.Ok(results);
    }
    catch (Exception e)
    {
        return Results.Problem("Internal server error");
    }
});

app.MapPost("emails/apple/send", async (
    Cache<string, SmtpClient> cache,
    [FromBody] EmailSendBodyDto body ) =>
{
    try
    {
        using var message = body.ToMimeMessage();

        var smtpClient = cache.Get(body.User);

        if (smtpClient is null)
        {
            smtpClient = new SmtpClient();
            
            var removed = cache.Add(body.User, smtpClient);
            
            if (removed is not null)
            {
                await removed.DisconnectAsync(true);
            }
        }

        if (!smtpClient.IsConnected)
        {
            await smtpClient.ConnectAsync(EmailServerConfig.AppleSmtpServerAddress, 587, SecureSocketOptions.StartTls);
    
            await smtpClient.AuthenticateAsync(body.User, body.Credentials);
        }
        
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

app.MapPost("emails/apple/details", async (
    Cache<string, ImapClient> cache,
    [FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = cache.Get(body.User);
    try
    {
        if (imapClient is null)
        {
            imapClient = new ImapClient();
            
            var removed = cache.Add(body.User, imapClient);

            if (removed is not null)
            {
                await removed.DisconnectAsync(true);
            }
        }

        var temporaryConnectionCreated = !imapClient.IsIdle;
        if (!imapClient.IsIdle)
        {
            imapClient = new ImapClient();
        }

        if (!imapClient.IsConnected)
        {
            await imapClient.ConnectAsync(EmailServerConfig.AppleImapServerAddress, 993, true);

            await imapClient.AuthenticateAsync(body.User, body.Credentials);
        }
        
        var inbox = imapClient.Inbox;

        if (!inbox.IsOpen)
        {
            await inbox.OpenAsync(FolderAccess.ReadOnly);
        }

        var message = await inbox.GetMessageAsync(body.EmailId);

        var results = EmailDetailsDto.FromMimeMessage(message);

        if (temporaryConnectionCreated)
        {
            await imapClient.DisconnectAsync(true);
        }

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
});

#endregion

#region google

app.MapPost("emails/google/list", async (
    Cache<string, ImapClient> cache,
    [FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = cache.Get(body.User);
    try
    {
        if (imapClient is null)
        {
            imapClient = new ImapClient();
            
            var removed = cache.Add(body.User, imapClient);

            if (removed is not null)
            {
                await removed.DisconnectAsync(true);
            }
        }
        
        var temporaryConnectionCreated = !imapClient.IsIdle;
        if (!imapClient.IsIdle)
        {
            imapClient = new ImapClient();
        }
        
        if (!imapClient.IsConnected)
        {
            await imapClient.ConnectAsync(EmailServerConfig.GoogleImapServerAddress, 993, true);

            await imapClient.AuthenticateAsync(new SaslMechanismOAuth2(body.User, body.Credentials));
        }

        var inbox = imapClient.Inbox;
        
        if (!inbox.IsOpen)
        {
            await inbox.OpenAsync(FolderAccess.ReadOnly);
        }
        
        var results = await inbox.FetchAsync(body.EmailsCount, body.Offset);

        if (temporaryConnectionCreated)
        {
            await imapClient.DisconnectAsync(true);
        }
        
        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
});

app.MapPost("emails/google/send", async (
    Cache<string, SmtpClient> cache,
    [FromBody] EmailSendBodyDto body) =>
{
    try
    {
        var message = body.ToMimeMessage();

        var smtpClient = cache.Get(body.User);
        
        if (smtpClient is null)
        {
            smtpClient = new SmtpClient();

            var removed = cache.Add(body.User, smtpClient);
            
            if (removed is not null)
            {
                await removed.DisconnectAsync(true);
            }
        }

        if (!smtpClient.IsConnected)
        {
            await smtpClient.ConnectAsync(EmailServerConfig.GoogleSmtpServerAddress, 587, SecureSocketOptions.StartTls);
    
            await smtpClient.AuthenticateAsync(new SaslMechanismOAuth2(body.User, body.Credentials));
        }
        
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

app.MapPost("emails/google/details", async (
    Cache<string, ImapClient> cache,
    [FromBody] EmailRequestBodyDto body) =>
{
    var imapClient = cache.Get(body.User);
    try
    {
        if (imapClient is null)
        {
            imapClient = new ImapClient();
            
            var removed = cache.Add(body.User, imapClient);

            if (removed is not null)
            {
                await removed.DisconnectAsync(true);
            }
        }
        
        var temporaryConnectionCreated = !imapClient.IsIdle;
        if (!imapClient.IsIdle)
        {
            imapClient = new ImapClient();
        }

        if (!imapClient.IsConnected)
        {
            await imapClient.ConnectAsync(EmailServerConfig.GoogleImapServerAddress, 993, true);

            await imapClient.AuthenticateAsync(new SaslMechanismOAuth2(body.User, body.Credentials));
        }
        
        var inbox = imapClient.Inbox;

        if (!inbox.IsOpen)
        {
            await inbox.OpenAsync(FolderAccess.ReadOnly);
        }
        
        var message = await inbox.GetMessageAsync(body.EmailId);

        var results = EmailDetailsDto.FromMimeMessage(message);

        if (temporaryConnectionCreated)
        {
            await imapClient.DisconnectAsync(true);
        }

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Internal server error");
    }
});


#endregion

app.Run();