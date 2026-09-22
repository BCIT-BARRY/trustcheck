```text
User need:
"I need to check one verification"

Action:
Read

HTTP method:
GET

Resource:
verifications

Specific resource:
{id}

Endpoint:
GET /api/verifications/{id}
```

GET /api/verifications/{id}

```json
{
    "id": "some-generated-id",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1900-01-01",
    "documentType": "Passport",
    "documentNumber": "2026-09-21-2213-1",
    "status": "Submitted",
    "result": null,
    "reason": null,
    "createdAt": "2026-09-21T21:27:00Z", // ISO 8601
    "completedAt": null
}
```

---------------
I need to see all customer verifications and their current statuses.
- GET
- READ
- /api/verifications
    - returns small summary;
        - id, firstName, lastName, status, result, createdAt
- Return what the client needs, not everyhing from the database. Security.