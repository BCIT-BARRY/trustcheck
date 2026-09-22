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
| Field | Description |
|---|---|
| Id | Unique ID for the verification |
| FirstName | Customer first name |
| LastName | Customer last name |
| DateOfBirth | Customer date of birth |
| Address | Customer address |
| Country | Customer country |
| DocumentType | Type of identity document |
| DocumentNumber | Document number |
| Status | Current verification status |
| Result | Final verification result |
| Reason | Explanation of the verification result |
| CreatedAt | When the verification was created |
| CompletedAt | When the verification finished |

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