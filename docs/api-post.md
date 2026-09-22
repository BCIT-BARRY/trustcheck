# POST /api/verifications
AT THE TIME OF SUBMISSION

## Request body: The Bank
```json
{
    "firstName": "Barry",
    "lastName": "Bui",
    "dateOfBirth": "1900-01-01",
    "address": "1234 St",
    "country": "Canada",
    "documentType": "Passport",
    "documentNumber": "2026-09-21-2127-1"
}
```
## Response body: The TrustCheck
```json
{
    "id": "some-generated-id",
    "status": "Submitted",
    "result": null,
    "reason": null,
    "createdAt": "2026-09-21T21:27:00Z", // ISO 8601
    "completedAt": null
}
```
Status Code:   
201 Created  
Location: /api/verifications/some-generated-id