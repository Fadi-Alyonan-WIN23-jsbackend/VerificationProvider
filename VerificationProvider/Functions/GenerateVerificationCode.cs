using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode
{
    private readonly ILogger<GenerateVerificationCode> _logger;
    private readonly IVerificationService _verificationService;
    public GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider, IVerificationService verificationService)
    {
        _logger = logger;
        _verificationService = verificationService;
    }

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task<string> Run(
        [ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var VerificationRequest = _verificationService.UnpackVerificationRequest(message);
            if (VerificationRequest != null)
            {
                var code = _verificationService.GenerateCode();
                if (!string.IsNullOrEmpty(code))
                {
                    var result = await _verificationService.SaveVerificationRequest(VerificationRequest, code);
                    if (result)
                    {
                        var emailRequest = _verificationService.GenerateEmailRequest(VerificationRequest, code);
                        if (emailRequest != null)
                        {
                            var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
                            if (!string.IsNullOrEmpty(payload))
                            {
                                await messageActions.CompleteMessageAsync(message);
                                return payload;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : GenerateVerificationCode.Run :: {ex.Message}");
        }
        return null!;
    }

    
}
