using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Functions;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class ValidateVerificationCodeService : IValidateVerificationCodeService
{
    private readonly ILogger<ValidateVerificationCode> _logger;
    private readonly DataContext _dataContext;

    public ValidateVerificationCodeService(ILogger<ValidateVerificationCode> logger, DataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }
    public async Task<bool> ValidatecodeAsync(ValidateRequest validateRequest)
    {
        try
        {
            var entity = await _dataContext.VerificatioRequests.FirstOrDefaultAsync(x => x.Email == validateRequest.Email && x.VerificationCode == validateRequest.Code);
            if (entity != null)
            {
                _dataContext.VerificatioRequests.Remove(entity);
                await _dataContext.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :  :: {ex.Message}");
        }
        return false;
    }

    public async Task<ValidateRequest> UnpackValidateRequest(HttpRequest req)
    {
        string body = null!;
        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var validateRequest = JsonConvert.DeserializeObject<ValidateRequest>(body)!;
                if (validateRequest != null)
                {
                    return validateRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($" StreamReader ValidateVerificationCode.UnpackValidateRequest :: {ex.Message}");
        }

        return null!;
    }
}
