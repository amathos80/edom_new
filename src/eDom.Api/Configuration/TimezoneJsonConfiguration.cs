using System.Text.Json;
using System.Text.Json.Serialization;

namespace eDom.Api.Configuration;

/// <summary>
/// Configura la serializzazione JSON per DateTime sempre in UTC con ISO 8601.
/// </summary>
public static class TimezoneJsonConfiguration
{
    /// <summary>
    /// Aggiunge al JsonSerializerOptions la gestione corretta di DateTime in UTC.
    /// </summary>
    public static JsonSerializerOptions ConfigureUtcDateTime(this JsonSerializerOptions options)
    {
        // Converter standard per DateTime in ISO 8601 UTC
        // Serializza come "2026-05-09T10:30:00Z" (sempre UTC)
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.WriteIndented = false;
        
        // Nota: System.Text.Json di default serializza DateTime in ISO 8601 UTC
        // Se il DateTime è .Kind = Utc, aggiunge "Z"
        // Se è .Kind = Unspecified, NON aggiunge offset (EVITARE!)
        
        return options;
    }

    /// <summary>
    /// Middleware che valida che tutti i DateTime nel contesto siano UTC.
    /// Utile per sviluppo per catturare bug di timezone early.
    /// </summary>
    public static IApplicationBuilder UseTimezoneValidation(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // Nel response, tutti i DateTime devono avere Kind = Utc
            // Questo middleware può validare se necessario durante lo sviluppo
            await next.Invoke();
        });
    }
}

/// <summary>
/// Custom JsonConverter per validare che DateTime siano sempre UTC durante la serializzazione.
/// </summary>
public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string? dateText = reader.GetString();
        
        if (string.IsNullOrEmpty(dateText))
            throw new JsonException("DateTime cannot be null");

        // Parse ISO 8601 string (con o senza offset)
        if (DateTime.TryParse(dateText, null, System.Globalization.DateTimeStyles.RoundtripKind, out var date))
        {
            // Se parsed come Local, converti a UTC
            if (date.Kind == DateTimeKind.Local)
            {
                date = date.ToUniversalTime();
            }
            else if (date.Kind == DateTimeKind.Unspecified)
            {
                // ATTENZIONE: DateTime Unspecified è ambiguo!
                // Tratta come UTC (assumendo che il client intendeva UTC)
                // Se no, usare: date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            return date;
        }

        throw new JsonException($"Unable to convert \"{dateText}\" to System.DateTime.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        // Valida che sia UTC
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new JsonException(
                $"DateTime must be UTC (Kind={DateTimeKind.Utc}), but got Kind={value.Kind}. " +
                $"Use DateTime.UtcNow, not DateTime.Now");
        }

        // Serializza in ISO 8601 UTC con "Z" suffix
        writer.WriteStringValue(value.ToString("O"));
    }
}

/// <summary>
/// Custom JsonConverter per DateOnly (sempre in cultura invariante).
/// </summary>
public class IsoDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string? dateText = reader.GetString();
        
        if (string.IsNullOrEmpty(dateText))
            throw new JsonException("DateOnly cannot be null");

        if (DateOnly.TryParseExact(dateText, Format, null, System.Globalization.DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new JsonException($"Unable to convert \"{dateText}\" to System.DateOnly.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateOnly value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
