using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class Paziente : IAuditableEntity
{
    // ── Identificazione ──────────────────────────────────────────────────────
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }

    // ── Anagrafica base ───────────────────────────────────────────────────────
    public string Cognome { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascita { get; set; }
    public string CodiceFiscale { get; set; } = string.Empty;
    public string Sesso { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string CodiceSanitario { get; set; } = string.Empty;
    public int CittadinanzaId { get; set; }
    public int ComuneNascitaId { get; set; }

    // ── Residenza ─────────────────────────────────────────────────────────────
    public int ComuneResidenzaId { get; set; }
    public string? CapResidenza { get; set; }
    public string? IndirizzoResidenza { get; set; }
    public int AreaResidenzaId { get; set; }

    // ── Domicilio ─────────────────────────────────────────────────────────────
    public int? ComuneDomicilioId { get; set; }
    public string? CapDomicilio { get; set; }
    public string? IndirizzoDomicilio { get; set; }
    public int? AreaDomicilioId { get; set; }

    // ── Reparto/struttura ─────────────────────────────────────────────────────
    public int? ComuneRepartoId { get; set; }
    public string? CapReparto { get; set; }
    public string? IndirizzoReparto { get; set; }
    public string? CameraReparto { get; set; }
    public int? AreaRepartoId { get; set; }

    // ── Contatti ──────────────────────────────────────────────────────────────
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? Telefono3 { get; set; }

    // ── Documento straniero ───────────────────────────────────────────────────
    public int? TipoDocumentoStranieroId { get; set; }
    public string? NumeroDocumentoStraniero { get; set; }
    public DateTime? ScadenzaDocumentoStraniero { get; set; }
    public string? NumeroTeamEuropeo { get; set; }
    public DateTime? ScadenzaTeamEuropeo { get; set; }

    // ── Dati sociali/anagrafici ────────────────────────────────────────────────
    public int? EsenzioneId { get; set; }
    public DateTime? DataEsenzione { get; set; }
    public int StatoCivileId { get; set; }
    public int? TitoloStudioId { get; set; }
    public int? ReligioneId { get; set; }
    public int? EventoAnagrafId { get; set; }
    public DateTime? DataEventoAnagrafe { get; set; }
    public int? ProfessioneId { get; set; }
    public int? CondizioneProfId { get; set; }
    public int? PosizioneProfId { get; set; }

    // ── Relazioni sanitarie ───────────────────────────────────────────────────
    public int? MedicoId { get; set; }
    public int? ConsultorioId { get; set; }

    // ── Genitore/Padre ────────────────────────────────────────────────────────
    public string? CognomePadre { get; set; }
    public string? NomePadre { get; set; }
    public string? CodiceFiscalePadre { get; set; }
    public DateTime? DataNascitaPadre { get; set; }
    public string? IndirizzoPadre { get; set; }
    public int? ComuneResidenzaPadreId { get; set; }
    public string? TelefonoPadre { get; set; }

    // ── Genitore/Madre ────────────────────────────────────────────────────────
    public string? CognomeMadre { get; set; }
    public string? NomeMadre { get; set; }
    public string? CodiceFiscaleMadre { get; set; }
    public DateTime? DataNascitaMadre { get; set; }
    public string? IndirizzoMadre { get; set; }
    public int? ComuneResidenzaMadreId { get; set; }
    public string? TelefonoMadre { get; set; }

    // ── Familiare 1 ───────────────────────────────────────────────────────────
    public string? CognomeFam1 { get; set; }
    public string? NomeFam1 { get; set; }
    public string? CodiceFiscaleFam1 { get; set; }
    public DateTime? DataNascitaFam1 { get; set; }
    public string? IndirizzoFam1 { get; set; }
    public int? ComuneResidenzaFam1Id { get; set; }
    public string? TelefonoFam1 { get; set; }

    // ── Campi alternativi / note ──────────────────────────────────────────────
    public string? CodiceAlternativo1 { get; set; }
    public string? CodiceAlternativo2 { get; set; }
    public string? Note { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }

    // ── Stato ─────────────────────────────────────────────────────────────────
    /// <summary>PAZI_F_ATT: 1 = attivo, 0 = disattivo (NUMBER(1) Oracle).</summary>
    public short Attivo { get; set; }

    // ── Computed (non persistito) ─────────────────────────────────────────────
    public string NomeCompleto => $"{Cognome} {Nome}".Trim();
    public bool IsAttivo => Attivo != 0;
}
