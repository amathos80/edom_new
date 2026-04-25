using System.Text.Json;
using eDom.Core.Interfaces;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eDom.Infrastructure.Interceptors;

/// <summary>
/// Intercetta ogni SaveChanges/SaveChangesAsync e scrive un record in SI_AUDIT_LOG
/// per ogni entità IAuditableEntity aggiunta, modificata o eliminata.
/// Il record viene scritto nella stessa transazione dell'entità principale.
/// Per operazioni bulk, usare SuppressAudit() / ResumeAudit() su HctDbContext.
/// </summary>
public sealed class AuditInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditRecords(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        AddAuditRecords(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void AddAuditRecords(DbContext? context)
    {
        if (context is null) return;

        // Supporto per SuppressAudit() su HctDbContext
        if (context is Data.HctDbContext hctCtx && hctCtx.AuditSuppressed) return;

        var now = DateTime.UtcNow;
        var userId = currentUser.Id;

        var entries = context.ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var operazione = entry.State switch
            {
                EntityState.Added    => "INS",
                EntityState.Modified => "UPD",
                EntityState.Deleted  => "DEL",
                _                    => null
            };
            if (operazione is null) continue;

            // Ricava l'EntitaId dalla PK (come stringa, supporta PK composite)
            var keyValues = entry.Metadata.FindPrimaryKey()
                ?.Properties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString())
                .ToArray();
            var entitaId = keyValues is not null ? string.Join("|", keyValues!) : string.Empty;

            string? valoriPrecedenti = null;
            string? valoriNuovi = null;

            if (entry.State == EntityState.Added)
            {
                valoriNuovi = SerializeValues(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else if (entry.State == EntityState.Modified)
            {
                // Serializza solo i campi effettivamente cambiati
                var changed = entry.Properties
                    .Where(p => p.IsModified)
                    .ToList();

                valoriPrecedenti = SerializeValues(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                valoriNuovi = SerializeValues(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else // Deleted
            {
                valoriPrecedenti = SerializeValues(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
            }

            context.Set<AuditLog>().Add(new AuditLog
            {
                Tabella           = entry.Metadata.ClrType.Name,
                EntitaId          = entitaId,
                Operazione        = operazione,
                UtenteId          = userId,
                DataOperazione    = now,
                ValoriPrecedenti  = valoriPrecedenti,
                ValoriNuovi       = valoriNuovi
            });
        }
    }

    private static string? SerializeValues(Dictionary<string, object?> values)
    {
        if (values.Count == 0) return null;
        return JsonSerializer.Serialize(values, _jsonOptions);
    }
}
