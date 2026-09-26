# TrustCheck - Barry Bui ✅
TrustCheck is an identity verification application that allows businesses to submit customer information for KYC (Know Your Customer) identity checks.

## Users
- Banks
- Fintechs
- Online Services

## Problem
TrustCheck helps organizations assess identity risk, reduce fraud risk, and maintain a record of verification results.

## Verification
A Verification represents one request to verify a customer's identity

## Lifecycle
- Status = Where the verification is in the process
  - Submitted
    - The customer has entered information and submitted verification request.
  - Verifying
    - TrustCheck is processing the verification.
  - Completed
    - Verification processing is finished and a result is available.

- Result = What the verification decided
  - Verified
  - Rejected

## Verification Fields
| Field | Description | Who |
|---|---|---|
| Id | Unique ID for the verification | TrustCheck |
| FirstName | Customer first name | Client |
| LastName | Customer last name | Client |
| DateOfBirth | Customer date of birth | Client |
| Address | Customer address | Client |
| Country | Customer country | Client |
| DocumentType | Type of identity document | Client |
| DocumentNumber | Document number | Client |
| Status | Current verification status | TrustCheck |
| Result | Final verification result | TrustCheck |
| Reason | Explanation of the verification result | TrustCheck|
| CreatedAt | When the verification was created | TrustCheck |
| CompletedAt | When the verification finished | TrustCheck |

## User Journey
I'm working at a bank;

- I need to submit customer identity information and document details for verification.
  - This creates a Verification with an initial status of `Submitted`.

- I need to check what happened to a verification I submitted.
  - I should be able to see its current `Status`.

- I need to see the result of a verification.
  - The result can be `Verified` or `Rejected`.

- I need to see all customer verifications and their current statuses.
  - I should be able to view all verification records together.


## Future Plans (WIP)
- Expand into KYB (Know your Business)
- Anti Money Launder (AML)
> TrustCheck uses fictional data only and is not a real identity or compliance product.

---

> [!IMPORTANT]
> Operated by bbdevhq © 2026 · Mandated by bbdevhq-policy
