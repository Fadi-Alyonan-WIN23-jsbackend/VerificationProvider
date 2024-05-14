using Azure.Messaging.ServiceBus;
using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class ValidateVerificationCode
{
    private readonly ILogger<ValidateVerificationCode> _logger;
    private readonly IValidateVerificationCodeService _validateVerificationCodeService;

    public ValidateVerificationCode(ILogger<ValidateVerificationCode> logger, IValidateVerificationCodeService validateVerificationCodeService)
    {
        _logger = logger;
        _validateVerificationCodeService = validateVerificationCodeService;
    }

    [Function("ValidateVerificationCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var validateRequest = await _validateVerificationCodeService.UnpackValidateRequest(req);
            if (validateRequest != null)
            {
                var validateRsult = await _validateVerificationCodeService.ValidatecodeAsync(validateRequest);
                if (validateRsult)
                {
                    return new OkResult();
                }
            }


        }
        catch (Exception ex)
        {
            _logger.LogError($"Error : ValidateVerificationCode :: {ex.Message}");
        }
        return new UnauthorizedResult();
    }

    
}
