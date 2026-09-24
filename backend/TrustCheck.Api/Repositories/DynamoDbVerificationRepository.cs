using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using TrustCheck.Api.Models;

namespace TrustCheck.Api.Repositories;

public class DynamoDbVerificationRepository : IVerificationRepository
{
    private const string StatusIndexName = "Status-CreatedAt-index";

    private readonly IAmazonDynamoDB _client;
    private readonly string _tableName;

    public DynamoDbVerificationRepository(
        IAmazonDynamoDB client,
        IConfiguration configuration)
    {
        _client = client;
        _tableName = configuration["DynamoDb:TableName"]
            ?? throw new InvalidOperationException(
                "DynamoDb:TableName is not configured.");
    }

    public async Task AddAsync(Verification verification)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = ToItem(verification),

            // Do not silently overwrite an existing verification.
            ConditionExpression = "attribute_not_exists(Id)"
        };

        await _client.PutItemAsync(request);
    }

    public async Task<Verification?> GetByIdAsync(Guid id)
    {
        var response = await _client.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        });

        if (response.Item is null || response.Item.Count == 0)
        {
            return null;
        }

        return FromItem(response.Item);
    }

    public async Task<IReadOnlyList<Verification>> GetAllAsync()
    {
        var verifications = new List<Verification>();
        Dictionary<string, AttributeValue>? lastKey = null;

        do
        {
            var request = new ScanRequest
            {
                TableName = _tableName
            };

            if (lastKey is { Count: > 0 })
            {
                request.ExclusiveStartKey = lastKey;
            }

            var response = await _client.ScanAsync(request);

            if (response.Items is not null)
            {
                verifications.AddRange(response.Items.Select(FromItem));
            }

            lastKey = response.LastEvaluatedKey;
        }
        while (lastKey is { Count: > 0 });

        return verifications;
    }

    public async Task<IReadOnlyList<Verification>> GetByStatusAsync(string status)
    {
        var verifications = new List<Verification>();
        Dictionary<string, AttributeValue>? lastKey = null;

        do
        {
            var request = new QueryRequest
            {
                TableName = _tableName,
                IndexName = StatusIndexName,
                KeyConditionExpression = "#status = :status",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#status"] = "Status"
                },
                ExpressionAttributeValues =
                    new Dictionary<string, AttributeValue>
                    {
                        [":status"] = new AttributeValue { S = status }
                    }
            };

            if (lastKey is { Count: > 0 })
            {
                request.ExclusiveStartKey = lastKey;
            }

            var response = await _client.QueryAsync(request);

            if (response.Items is not null)
            {
                verifications.AddRange(response.Items.Select(FromItem));
            }

            lastKey = response.LastEvaluatedKey;
        }
        while (lastKey is { Count: > 0 });

        return verifications;
    }

    public async Task UpdateAsync(Verification verification)
    {
        await _client.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToItem(verification)
        });
    }

    private static Dictionary<string, AttributeValue> ToItem(
        Verification verification)
    {
        return new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue
            {
                S = verification.Id.ToString()
            },

            ["FirstName"] = new AttributeValue
            {
                S = verification.FirstName
            },

            ["LastName"] = new AttributeValue
            {
                S = verification.LastName
            },

            ["DateOfBirth"] = new AttributeValue
            {
                S = verification.DateOfBirth.ToString("yyyy-MM-dd")
            },

            ["Address"] = new AttributeValue
            {
                S = verification.Address
            },

            ["Country"] = new AttributeValue
            {
                S = verification.Country
            },

            ["DocumentType"] = new AttributeValue
            {
                S = verification.DocumentType
            },

            ["DocumentNumber"] = new AttributeValue
            {
                S = verification.DocumentNumber
            },

            ["Status"] = new AttributeValue
            {
                S = verification.Status
            },

            ["Result"] = verification.Result is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { S = verification.Result },

            ["Reason"] = verification.Reason is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue { S = verification.Reason },

            ["CreatedAt"] = new AttributeValue
            {
                S = verification.CreatedAt.ToString("O")
            },

            ["CompletedAt"] = verification.CompletedAt is null
                ? new AttributeValue { NULL = true }
                : new AttributeValue
                {
                    S = verification.CompletedAt.Value.ToString("O")
                }
        };
    }

    private static Verification FromItem(
        Dictionary<string, AttributeValue> item)
    {
        return new Verification
        {
            Id = Guid.Parse(item["Id"].S),

            FirstName = item["FirstName"].S,
            LastName = item["LastName"].S,

            DateOfBirth = DateOnly.ParseExact(
                item["DateOfBirth"].S,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture),

            Address = item["Address"].S,
            Country = item["Country"].S,
            DocumentType = item["DocumentType"].S,
            DocumentNumber = item["DocumentNumber"].S,
            Status = item["Status"].S,

            Result = GetNullableString(item, "Result"),
            Reason = GetNullableString(item, "Reason"),

            CreatedAt = DateTimeOffset.Parse(
                item["CreatedAt"].S,
                CultureInfo.InvariantCulture),

            CompletedAt = GetNullableDateTimeOffset(
                item,
                "CompletedAt")
        };
    }

private static string? GetNullableString(
    Dictionary<string, AttributeValue> item,
    string key)
{
    if (!item.TryGetValue(key, out var value) ||
        value is null ||
        value.NULL == true)
    {
        return null;
    }

    return value.S;
}

    private static DateTimeOffset? GetNullableDateTimeOffset(
        Dictionary<string, AttributeValue> item,
        string key)
    {
        var value = GetNullableString(item, key);

        return value is null
            ? null
            : DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture);
    }
}