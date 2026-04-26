namespace eDom.Core.Entities;

/// <summary>Layout dashboard personalizzato per utente, serializzato come JSON.</summary>
public class UserDashboardLayout
{
    public int Id { get; set; }

    /// <summary>Codice univoco dell'utente (corrisponde a JWT sub/unique_name).</summary>
    public string UserCodice { get; set; } = string.Empty;

    /// <summary>Layout serializzato come JSON.</summary>
    public string LayoutJson { get; set; } = "{}";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
