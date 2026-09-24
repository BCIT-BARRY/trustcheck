namespace TrustCheck.Api.Dtos;

public class CreateVerificationResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}