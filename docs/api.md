# TrustCheck API

TrustCheck exposes an HTTP API for creating, retrieving, listing, and processing identity verifications.

## Verification Lifecycle

```text
Submitted → Verifying → Completed
```

### Status

- `Submitted` — Verification has been created and is waiting to be processed.
- `Verifying` — TrustCheck is processing the verification.
- `Completed` — Processing has finished and a result is available.

### Result

A result is only available when `status` is `Completed`.

Possible values:

- `Verified`
- `Rejected`

Before completion:

```json
{
  "result": null,
  "reason": null,
  "completedAt": null
}
```

## Supported Document Types

- `Passport`
- `DriverLicence`

---

# POST /api/verifications

Creates a new Verification.

## Request

The client provides customer information.

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "dateOfBirth": "1995-01-01",
  "address": "123 Fake Street",
  "country": "Canada",
  "documentType": "Passport",
  "documentNumber": "TEST123"
}
```

The client does **not** provide:

- `id`
- `status`
- `result`
- `reason`
- `createdAt`
- `completedAt`

These fields are controlled by TrustCheck.

## Successful Response

```http
201 Created
```

```json
{
  "id": "generated-verification-id",
  "status": "Submitted",
  "result": null,
  "reason": null,
  "createdAt": "2026-09-22T21:27:00Z",
  "completedAt": null
}
```

Response header:

```http
Location: /api/verifications/generated-verification-id
```

## Validation

The request returns `400 Bad Request` when input is invalid.

Current MVP rules:

```text
FirstName
→ required, at most 100 characters

LastName
→ required, at most 100 characters

DateOfBirth
→ required, not in the future, not before 1900

Address
→ required, at most 200 characters

Country
→ required, at most 60 characters

DocumentType
→ required and must be Passport or DriverLicence

DocumentNumber
→ required, at most 30 characters
```

Lengths are measured after surrounding spaces are trimmed.

Example:

```http
400 Bad Request
```

```json
{
  "code": "validation_error",
  "message": "firstName is required."
}
```

---

# GET /api/verifications/{id}

Returns one complete Verification.

## Successful Response

```http
200 OK
```

```json
{
  "id": "test-001",
  "firstName": "Jane",
  "lastName": "Doe",
  "dateOfBirth": "1995-01-01",
  "address": "123 Fake Street",
  "country": "Canada",
  "documentType": "Passport",
  "documentNumber": "TEST123",
  "status": "Submitted",
  "result": null,
  "reason": null,
  "createdAt": "2026-09-22T21:20:00Z",
  "completedAt": null
}
```

## Verification Not Found

```http
404 Not Found
```

```json
{
  "code": "verification_not_found",
  "message": "Verification was not found."
}
```

---

# GET /api/verifications

Returns Verification summaries.

The list endpoint intentionally returns fewer fields than the single-resource endpoint.

## Successful Response

```http
200 OK
```

```json
[
  {
    "id": "test-001",
    "firstName": "Jane",
    "lastName": "Doe",
    "status": "Submitted",
    "result": null,
    "createdAt": "2026-09-22T21:20:00Z"
  },
  {
    "id": "test-004",
    "firstName": "Taylor",
    "lastName": "Brown",
    "status": "Completed",
    "result": "Verified",
    "createdAt": "2026-09-22T21:35:00Z"
  }
]
```

If no Verifications exist:

```http
200 OK
```

```json
[]
```

An empty collection is not an API error. The frontend may display an empty state such as:

```text
No verifications yet.
```

## Filter by Status

```http
GET /api/verifications?status=Submitted
```

`status` is optional. Allowed values are `Submitted`, `Verifying` and `Completed`.

Any other value returns:

```http
400 Bad Request
```

```json
{
  "code": "validation_error",
  "message": "Status must be Submitted, Verifying or Completed."
}
```

---

# POST /api/verifications/{id}/run

Starts processing a submitted Verification.

## Submitted Verification

State transition:

```text
Submitted → Verifying
```

Response:

```http
202 Accepted
```

```json
{
  "id": "test-001",
  "status": "Verifying",
  "result": null
}
```

Processing eventually finishes as:

```text
Verifying
    ↓
Completed
    ↓
Verified OR Rejected
```

## Already Verifying

A Verification cannot be started again while processing.

```http
409 Conflict
```

```json
{
  "code": "verification_in_progress",
  "message": "Verification is already in progress."
}
```

## Already Completed

A completed Verification cannot be run again in the MVP.

```http
409 Conflict
```

```json
{
  "code": "verification_already_completed",
  "message": "Verification has already been completed."
}
```

## Verification Not Found

```http
404 Not Found
```

```json
{
  "code": "verification_not_found",
  "message": "Verification was not found."
}
```

---

# Error Format

All API errors use the same JSON structure:

```json
{
  "code": "machine_readable_code",
  "message": "Human-readable explanation."
}
```

## HTTP Status Codes

```text
200 OK
→ Request succeeded.

201 Created
→ A new Verification was created.

202 Accepted
→ Processing was accepted and has started.

400 Bad Request
→ Request data is invalid.

404 Not Found
→ The requested Verification does not exist.

409 Conflict
→ The Verification exists, but the requested action conflicts
  with its current state.
```

---

# Data Ownership

```text
Client-controlled
-----------------
FirstName
LastName
DateOfBirth
Address
Country
DocumentType
DocumentNumber

TrustCheck-controlled
---------------------
Id
Status
Result
Reason
CreatedAt
CompletedAt
```

TrustCheck generates the Verification ID and controls all lifecycle and result fields.