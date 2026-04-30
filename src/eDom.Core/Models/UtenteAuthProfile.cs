namespace eDom.Core.Models;

public sealed class ProfiloAutorizzativoUtente
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public short FlagCambiaPwd { get; set; }
    public DateTime? DataDisattivazione { get; set; }
    public DateTime? DataScadenzaPassword { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public IList<string> Ruoli { get; set; } = [];
    public IList<string> Funzioni { get; set; } = [];

    public string NomeCompleto => $"{Nome} {Cognome}".Trim();
}