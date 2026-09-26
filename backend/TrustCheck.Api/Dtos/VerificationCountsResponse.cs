namespace TrustCheck.Api.Dtos;

public class VerificationCountsResponse
{
    public int Total { get; set; }
    public int Submitted { get; set; }
    public int Verifying { get; set; }
    public int Completed { get; set; }
    public int Verified { get; set; }
    public int Rejected { get; set; }
}
