namespace eDom.Core.Entities;

/// <summary>Traccia ogni accesso riuscito al sistema. Mappa su SI_LOGACC.</summary>
public class LogAccesso
{
    public int Id { get; set; }
    public int UtenteId { get; set; }
    public DateTime Data { get; set; }
    public string? IndirizzoIp { get; set; }
    public string? NomeMacchina { get; set; }
    /// <summary>ID procedura di contesto (999999 = login generico).</summary>
    public int ProcedureId { get; set; }
    /// <summary>ID funzione di contesto (999999 = login generico).</summary>
    public int FunzioneId { get; set; }
}
