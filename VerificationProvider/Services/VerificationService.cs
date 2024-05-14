using Azure.Messaging.ServiceBus;
using Data.Context;
using EmailProvider.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Functions;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService : IVerificationService
{
    private readonly ILogger<GenerateVerificationCode> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VerificationService(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificationCode.GenerateServiceBusEmailRequest :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();
            var existingRequest = await context.VerificatioRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
            if (existingRequest != null)
            {
                existingRequest.VerificationCode = code;
                existingRequest.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificatioRequests.Add(new Data.Entities.VerificatioRequestEntity() { Email = verificationRequest.Email, VerificationCode = code });
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificationCode.SaveVerificationRequest :: {ex.Message}");
        }
        return false;
    }

    public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var VerificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (VerificationRequest != null && !string.IsNullOrEmpty(VerificationRequest.Email))
            {
                return VerificationRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificationCode.UnpackVerificationRequest :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var verificationCode = rnd.Next(100000, 999999);
            return verificationCode.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificatioCode.GenerateCode  :: {ex.Message}");
        }
        return null!;
    }

    public EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(verificationRequest.Email))
            {
                var emailRequest = new EmailRequest()
                {
                    to = verificationRequest.Email,
                    subject = $"Verification code {code}",
                    HTMLbody = $@"<html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Verification Code</title>
                        </head>
                        <body>
                            <div style='color: #191919; max-width: 500px;'>
                                <div style='background-color: #4f85f6; color: white; text-align: center; padding: 20px 0;'>
                                    <h1 style='font-weight: 400;'>Verification code</h1>
                                </div>
                                <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                    <p>Dear {verificationRequest.Email}</p>
                                    <p>we received a request to sign in to your account using e-mail {verificationRequest.Email}. please verify your account using this Verification code: </p>
                                    <p class='code' style='font-weight: 700; text-align: center; font-size: 48px; letter-spacing: 8px;'>
                                    {code}
                                    </p>
                                    <div class='noreply' style='color: #191919; font-size: 11px;'>
                                        <p>If you did not request this code, it is possible that someone else is trying to access the Silicon account <span style='color: #4f85f6;'>{verificationRequest.Email}.</span> This email can't receive replies. For more information, contact the Silicon Help Center</p>
                                    </div>
                                </div>
                                <div style='color: #191919; text-align: center; font-size: 11px;'>
                                <p>copy; Silicon, Sveavägen 1, se-123 45 Stockholm, sweden</p>
                                </div>

                            </div>
                        </body>
                        </html>",
                    Text = $"please verify your account using this Verification code: {code}. If you did not request this code, it is possible that someone else is trying to access the Silicon account {verificationRequest.Email}. This email can't receive replies. For more information, contact the Silicon Help Center "
                };
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificatioCode.GenerateEmailRequest  :: {ex.Message}");
        }
        return null!;
    }
}
