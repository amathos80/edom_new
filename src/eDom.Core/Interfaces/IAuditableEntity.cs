namespace eDom.Core.Interfaces;

/// <summary>
/// Marker interface. Le entità che la implementano vengono tracciate automaticamente
/// dall'AuditInterceptor: ogni INSERT, UPDATE o DELETE genera un record in SI_AUDIT_LOG.
/// Non implementare su entità di lookup statiche (comuni, province, tabelle di configurazione).
/// </summary>
public interface IAuditableEntity { }
