namespace eDom.Core.Entities;

/// <summary>Messaggio di sistema multilingua. Mappa su SI_SISMESS.</summary>
public class SistemaMessaggio
{
    public int Id { get; set; }
    public string Classe { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;
    public string Lingua { get; set; } = string.Empty;
    public string? Custom01 { get; set; }
    public string? Custom02 { get; set; }
    public string? Custom03 { get; set; }
    public string? Custom04 { get; set; }
    public string? Custom05 { get; set; }
    public short FlagAttivo { get; set; }
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }
}
