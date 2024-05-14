using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class VerificatioRequestEntity
{
    [Key]
    public string Email { get; set; } = null!;
    public string VerificationCode { get; set; } = null!;
    public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMinutes(5);
}
