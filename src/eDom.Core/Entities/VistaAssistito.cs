namespace eDom.Core.Entities;

/// <summary>
/// Read-only entity mapped to the V_ANAGRAFE_ASSISTITI view.
/// Contains all columns needed for patient search, display, and creation from assistiti.
/// </summary>
public class VistaAssistito
{
    // ── Identificazione ──────────────────────────────────────────────────────
    public string? AaId { get; set; }

    // ── Anagrafica base ───────────────────────────────────────────────────────
    public string? Cognome { get; set; }
    public string? Nome { get; set; }
    public DateOnly? DataNascita { get; set; }
    public string? CodiceFiscale { get; set; }
    public string? Sesso { get; set; }
    public string? Email { get; set; }
    public string? CodiceSanitario { get; set; }

    // ── Nascita ───────────────────────────────────────────────────────────────
    public int? NascitaCodComune { get; set; }
    public string? NascitaDescrComune { get; set; }

    // ── Cittadinanza ──────────────────────────────────────────────────────────
    public int? CittadinanzaCod { get; set; }
    public string? CittadinanzaDescr { get; set; }

    // ── Residenza ─────────────────────────────────────────────────────────────
    public int? ResidenzaCodComune { get; set; }
    public string? ResidenzaDescrComune { get; set; }
    public string? ResidenzaIndirizzo { get; set; }
    public string? ResidenzaCap { get; set; }
    public int? ResidenzaArea { get; set; }

    // ── Domicilio ─────────────────────────────────────────────────────────────
    public int? DomicilioCodComune { get; set; }
    public string? DomicilioDescrComune { get; set; }
    public string? DomicilioIndirizzo { get; set; }
    public string? DomicilioCap { get; set; }
    public int? DomicilioArea { get; set; }

    // ── Reperibilità ──────────────────────────────────────────────────────────
    public int? ReperibilitaCodComune { get; set; }
    public string? ReperibilitaDescrComune { get; set; }
    public string? ReperibilitaIndirizzo { get; set; }
    public string? ReperibilitaCap { get; set; }
    public string? ReperibilitaNomeCampanello { get; set; }
    public int? ReperibilitaArea { get; set; }

    // ── Contatti ──────────────────────────────────────────────────────────────
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? Telefono3 { get; set; }

    // ── Stato civile e medico ─────────────────────────────────────────────────
    public int? StatoCivileCod { get; set; }
    public int? MmgCod { get; set; }
}
