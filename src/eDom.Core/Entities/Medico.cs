namespace eDom.Core.Entities;

public class Medico
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string? Cognome { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
}
