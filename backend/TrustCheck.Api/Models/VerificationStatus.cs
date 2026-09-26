namespace TrustCheck.Api.Models;

public static class VerificationStatus
{
    public const string Submitted = "Submitted";
    public const string Verifying = "Verifying";
    public const string Completed = "Completed";

    public static readonly string[] All = { Submitted, Verifying, Completed };
}
