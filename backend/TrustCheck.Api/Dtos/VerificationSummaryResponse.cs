namespace TrustCheck.Api.Dtos;

public class VerificationSummaryResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}