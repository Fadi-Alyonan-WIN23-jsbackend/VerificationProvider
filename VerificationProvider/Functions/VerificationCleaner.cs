using System;
using Data.Context;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VerificationProvider.Functions
{
    public class VerificationCleaner
    {
        private readonly ILogger<VerificationCleaner> _logger;
        private readonly DataContext _dataContext;

        public VerificationCleaner(ILogger<VerificationCleaner> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [Function("VerificationCleaner")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            try
            {
                var expired = await _dataContext.VerificatioRequests.Where(x => x.ExpiryDate <= DateTime.Now).ToListAsync();
                _dataContext.Remove(expired);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : VerificationCleaner :: {ex.Message}");
            }

        }
    }
}
