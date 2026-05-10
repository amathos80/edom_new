namespace eDom.Core.Entities;

public class NumeroPua
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public int Anno { get; set; }

    /// <summary>Display label: Codice/Anno</summary>
    public string CodiceAnno => $"{Codice}/{Anno}";
}
