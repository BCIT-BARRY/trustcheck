# Local Development

## Requirements

* .NET 10 SDK
* Docker
* AWS CLI
* Node.js 22 and pnpm (release scripts only)

## 1. Start DynamoDB Local

```bash
docker run -p 8000:8000 amazon/dynamodb-local
```

`appsettings.Development.json` points the API at `http://localhost:8000`.

## 2. Create the table

`Verifications` uses `Id` as its key. `Status-CreatedAt-index` serves lookups by status.

```bash
aws dynamodb create-table \
  --table-name Verifications \
  --attribute-definitions AttributeName=Id,AttributeType=S AttributeName=Status,AttributeType=S AttributeName=CreatedAt,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --global-secondary-indexes "IndexName=Status-CreatedAt-index,KeySchema=[{AttributeName=Status,KeyType=HASH},{AttributeName=CreatedAt,KeyType=RANGE}],Projection={ProjectionType=ALL}" \
  --billing-mode PAY_PER_REQUEST \
  --endpoint-url http://localhost:8000
```

DynamoDB Local accepts any credentials.

## 3. Load seed data

```bash
aws dynamodb batch-write-item \
  --request-items file://seed-verifications.json \
  --endpoint-url http://localhost:8000
```

## 4. Run the API

```bash
dotnet run --project backend/TrustCheck.Api
```

The API listens on `http://localhost:5086`. `backend/TrustCheck.Api/TrustCheck.Api.http` holds a request for every endpoint.

## 5. Run the tests

```bash
dotnet test trustcheck.sln
```

Tests use the in memory repository and do not require Docker.

## Rules

* Document numbers ending in `0` are rejected. All others are verified.
* `POST /api/verifications/{id}/run` moves a verification from `Submitted` to `Verifying`. The background worker completes it after one second.
* `frontend/` is static HTML, CSS and JavaScript served from any static server.
