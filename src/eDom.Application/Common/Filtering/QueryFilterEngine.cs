using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;

namespace eDom.Application.Common.Filtering;

public static class QueryFilterEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static FilterGroup? Parse(string? filterJson)
    {
        if (string.IsNullOrWhiteSpace(filterJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<FilterGroup>(filterJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static Expression<Func<T, bool>>? BuildPredicate<T>(
        FilterGroup group,
        IReadOnlyDictionary<string, string>? fieldMap = null)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var body = BuildGroupBody<T>(group, parameter, fieldMap);
        return body is null ? null : Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression? BuildGroupBody<T>(
        FilterGroup group,
        ParameterExpression parameter,
        IReadOnlyDictionary<string, string>? fieldMap)
    {
        var conditionBodies = group.Conditions
            .Select(c => BuildConditionBody<T>(c, parameter, fieldMap))
            .Where(expr => expr is not null)
            .Cast<Expression>()
            .ToList();

        var nestedGroupBodies = (group.Groups ?? [])
            .Select(g => BuildGroupBody<T>(g, parameter, fieldMap))
            .Where(expr => expr is not null)
            .Cast<Expression>()
            .ToList();

        var all = conditionBodies.Concat(nestedGroupBodies).ToList();
        if (all.Count == 0)
        {
            return null;
        }

        var logic = (group.Logic ?? "and").ToLowerInvariant();
        return all.Aggregate((left, right) => logic == "or"
            ? Expression.OrElse(left, right)
            : Expression.AndAlso(left, right));
    }

    private static Expression? BuildConditionBody<T>(
        FilterCondition condition,
        ParameterExpression parameter,
        IReadOnlyDictionary<string, string>? fieldMap)
    {
        if (string.IsNullOrWhiteSpace(condition.Field) || string.IsNullOrWhiteSpace(condition.Op))
        {
            return null;
        }

        var propertyPath = ResolvePropertyPath(condition.Field, fieldMap);
        var member = BuildMemberExpression(parameter, propertyPath);
        if (member is null)
        {
            return null;
        }

        var op = condition.Op.ToLowerInvariant();
        var declaredType = condition.Type?.ToLowerInvariant();
        var memberType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

        if (memberType == typeof(string))
        {
            return BuildStringCondition(member, op, condition);
        }

        if (memberType == typeof(DateTime))
        {
            return BuildComparableCondition(member, memberType, op, condition, TryReadDateTime);
        }

        if (memberType == typeof(bool))
        {
            return BuildBooleanCondition(member, op, condition);
        }

        if (IsNumericType(memberType))
        {
            if (declaredType == "boolean")
            {
                return BuildNumericBoolCondition(member, memberType, op, condition);
            }

            return BuildComparableCondition(member, memberType, op, condition,
                (JsonElement element, out object? value) => TryReadNumber(element, memberType, out value));
        }

        return null;
    }

    private static string ResolvePropertyPath(string field, IReadOnlyDictionary<string, string>? fieldMap)
    {
        if (fieldMap is not null && fieldMap.TryGetValue(field, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
        {
            return mapped;
        }

        return field;
    }

    private static MemberExpression? BuildMemberExpression(Expression parameter, string propertyPath)
    {
        var current = parameter;
        foreach (var part in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var property = current.Type.GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, part, StringComparison.OrdinalIgnoreCase));
            if (property is null)
            {
                return null;
            }

            current = Expression.Property(current, property);
        }

        return current as MemberExpression;
    }

    private static Expression? BuildStringCondition(MemberExpression member, string op, FilterCondition condition)
    {
        var normalizedMember = Expression.Call(
            Expression.Coalesce(member, Expression.Constant(string.Empty)),
            typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);

        if (op == "in")
        {
            var values = ReadArray(condition)
                .Select(v => v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.ToLowerInvariant())
                .Distinct()
                .ToList();

            if (values.Count == 0)
            {
                return null;
            }

            Expression? result = null;
            foreach (var itemValue in values)
            {
                var eq = Expression.Equal(normalizedMember, Expression.Constant(itemValue));
                result = result is null ? eq : Expression.OrElse(result, eq);
            }

            return result;
        }

        var raw = ReadSingleValue(condition);
        if (!raw.HasValue)
        {
            return null;
        }

        var value = raw.Value.ValueKind == JsonValueKind.String ? raw.Value.GetString() : raw.Value.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = Expression.Constant(value.ToLowerInvariant());

        return op switch
        {
            "eq" => Expression.Equal(normalizedMember, normalizedValue),
            "ne" => Expression.NotEqual(normalizedMember, normalizedValue),
            "contains" => Expression.Call(normalizedMember, nameof(string.Contains), Type.EmptyTypes, normalizedValue),
            "startswith" => Expression.Call(normalizedMember, nameof(string.StartsWith), Type.EmptyTypes, normalizedValue),
            "endswith" => Expression.Call(normalizedMember, nameof(string.EndsWith), Type.EmptyTypes, normalizedValue),
            _ => null
        };
    }

    private static Expression? BuildBooleanCondition(MemberExpression member, string op, FilterCondition condition)
    {
        var raw = ReadSingleValue(condition);
        if (!raw.HasValue || !TryReadBool(raw.Value, out var parsed))
        {
            return null;
        }

        var constant = Expression.Constant(parsed, member.Type);
        return op switch
        {
            "eq" => Expression.Equal(member, constant),
            "ne" => Expression.NotEqual(member, constant),
            _ => null
        };
    }

    private static Expression? BuildNumericBoolCondition(MemberExpression member, Type memberType, string op, FilterCondition condition)
    {
        var raw = ReadSingleValue(condition);
        if (!raw.HasValue || !TryReadBool(raw.Value, out var parsed))
        {
            return null;
        }

        var numericValue = parsed ? 1 : 0;
        var typedConstant = Expression.Constant(Convert.ChangeType(numericValue, memberType, CultureInfo.InvariantCulture), member.Type);

        return op switch
        {
            "eq" => Expression.Equal(member, typedConstant),
            "ne" => Expression.NotEqual(member, typedConstant),
            _ => null
        };
    }

    private static Expression? BuildComparableCondition(
        MemberExpression member,
        Type memberType,
        string op,
        FilterCondition condition,
        TryReadValue reader)
    {
        if (op == "in")
        {
            var values = ReadArray(condition)
                .Select(v => reader(v, out var converted) ? converted : null)
                .Where(v => v is not null)
                .Cast<object>()
                .Distinct()
                .ToList();

            if (values.Count == 0)
            {
                return null;
            }

            Expression? result = null;
            foreach (var item in values)
            {
                var constant = Expression.Constant(item, member.Type);
                var eq = Expression.Equal(member, constant);
                result = result is null ? eq : Expression.OrElse(result, eq);
            }

            return result;
        }

        if (op == "between")
        {
            var values = ReadArray(condition).ToList();
            if (values.Count < 2)
            {
                return null;
            }

            if (!reader(values[0], out var from) || !reader(values[1], out var to) || from is null || to is null)
            {
                return null;
            }

            var fromConstant = Expression.Constant(from, member.Type);
            var toConstant = Expression.Constant(to, member.Type);

            return Expression.AndAlso(
                Expression.GreaterThanOrEqual(member, fromConstant),
                Expression.LessThanOrEqual(member, toConstant));
        }

        var raw = ReadSingleValue(condition);
        if (!raw.HasValue || !reader(raw.Value, out var parsed) || parsed is null)
        {
            return null;
        }

        var constantValue = Expression.Constant(parsed, member.Type);

        return op switch
        {
            "eq" => Expression.Equal(member, constantValue),
            "ne" => Expression.NotEqual(member, constantValue),
            "gt" => Expression.GreaterThan(member, constantValue),
            "gte" => Expression.GreaterThanOrEqual(member, constantValue),
            "lt" => Expression.LessThan(member, constantValue),
            "lte" => Expression.LessThanOrEqual(member, constantValue),
            _ => null
        };
    }

    private delegate bool TryReadValue(JsonElement element, out object? value);

    private static JsonElement? ReadSingleValue(FilterCondition condition)
    {
        if (condition.Value.HasValue &&
            condition.Value.Value.ValueKind != JsonValueKind.Null &&
            condition.Value.Value.ValueKind != JsonValueKind.Undefined)
        {
            return condition.Value.Value;
        }

        return null;
    }

    private static IEnumerable<JsonElement> ReadArray(FilterCondition condition)
    {
        if (condition.Values is { Count: > 0 })
        {
            return condition.Values;
        }

        if (condition.Value.HasValue && condition.Value.Value.ValueKind == JsonValueKind.Array)
        {
            return condition.Value.Value.EnumerateArray();
        }

        return [];
    }

    private static bool TryReadBool(JsonElement element, out bool value)
    {
        value = default;

        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
        {
            value = element.GetBoolean();
            return true;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var n))
        {
            if (n is 0 or 1)
            {
                value = n == 1;
                return true;
            }

            return false;
        }

        return bool.TryParse(element.ToString(), out value);
    }

    private static bool TryReadNumber(JsonElement element, Type targetType, out object? value)
    {
        value = null;
        var raw = element.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        try
        {
            var parsed = Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
            value = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadDateTime(JsonElement element, out object? value)
    {
        value = null;
        if (!DateTime.TryParse(element.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return false;
        }

        value = parsed;
        return true;
    }

    private static bool IsNumericType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type == typeof(byte)
            || type == typeof(short)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal);
    }
}
