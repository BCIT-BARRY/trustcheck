# Local Development

How I run TrustCheck on my machine.

## What you need

* .NET 10 SDK
* Docker (for DynamoDB Local)
* AWS CLI (only used to create the table and load test data)
* Node.js 22 and pnpm (only for the release scripts)

## 1. Start DynamoDB Local

```bash
docker run -p 8000:8000 amazon/dynamodb-local
```

The API talks to it through `DynamoDb:ServiceUrl` in `appsettings.Development.json` (`http://localhost:8000`).

## 2. Create the table

The table is called `Verifications`. `Id` is the key, and `Status-CreatedAt-index` lets the API look up verifications by status.

```bash
aws dynamodb create-table \
  --table-name Verifications \
  --attribute-definitions AttributeName=Id,AttributeType=S AttributeName=Status,AttributeType=S AttributeName=CreatedAt,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --global-secondary-indexes "IndexName=Status-CreatedAt-index,KeySchema=[{AttributeName=Status,KeyType=HASH},{AttributeName=CreatedAt,KeyType=RANGE}],Projection={ProjectionType=ALL}" \
  --billing-mode PAY_PER_REQUEST \
  --endpoint-url http://localhost:8000
```

DynamoDB Local accepts any credentials, so `aws configure` with fake values is fine.

## 3. Load the test data (optional)

```bash
aws dynamodb batch-write-item \
  --request-items file://seed-verifications.json \
  --endpoint-url http://localhost:8000
```

## 4. Run the API

```bash
dotnet run --project backend/TrustCheck.Api
```

It listens on `http://localhost:5086`. `backend/TrustCheck.Api/TrustCheck.Api.http` has ready requests for every endpoint.

## 5. Run the tests

```bash
dotnet test trustcheck.sln
```

The tests use the in memory repository, so they do not need Docker.

## Useful notes

* Document numbers ending in `0` are rejected by the demo verification rule. Everything else is verified.
* A verification only moves from `Submitted` to `Verifying` when you call `POST /api/verifications/{id}/run`. The background worker finishes it about one second later.
* The landing page in `frontend/` is plain HTML, CSS and JavaScript. Open `frontend/index.html` in a browser, or serve the folder with any static server.
