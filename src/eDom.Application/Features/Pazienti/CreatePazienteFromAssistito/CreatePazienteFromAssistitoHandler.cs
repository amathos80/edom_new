using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using System.Globalization;

namespace eDom.Application.Features.Pazienti;

public sealed class CreatePazienteFromAssistitoHandler(
    IAssistitiRepository assistitiRepository,
    IPazientiRepository pazientiRepository,
    PazientiMapper mapper,
    ICurrentUser currentUser)
    : IRequestHandler<CreatePazienteFromAssistitoCommand, PazienteDto>
{
    public async Task<PazienteDto> HandleAsync(CreatePazienteFromAssistitoCommand cmd, CancellationToken ct = default)
    {
        // Leggi l'assistito usando il repository
        var assistito = await assistitiRepository.GetByIdAsync(cmd.AssistitoId, ct);
            
        if (assistito is null)
            throw new InvalidOperationException($"Assistito con ID {cmd.AssistitoId} non trovato.");

        var nextPazienteId = await pazientiRepository.ReserveNextIdAsync(ct);

        // Crea il paziente dai dati dell'assistito con mapping completo di TUTTI i campi disponibili
        var paziente = new Paziente
        {
            // ── Identificazione ──────────────────────────────────────────────
            Id = nextPazienteId,
            Codice = nextPazienteId.ToString(CultureInfo.InvariantCulture),
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            ValidTo = DateOnly.FromDateTime(DateTime.MaxValue),

            // ── Anagrafica base ──────────────────────────────────────────────
            Cognome = assistito.Cognome ?? string.Empty,
            Nome = assistito.Nome ?? string.Empty,
            DataNascita = assistito.DataNascita ?? DateOnly.MinValue,
            CodiceFiscale = assistito.CodiceFiscale ?? string.Empty,
            Sesso = assistito.Sesso ?? "M",
            Email = assistito.Email,
            CodiceSanitario = assistito.CodiceSanitario ?? string.Empty,

            // ── Identificazione geografica ───────────────────────────────────
            CittadinanzaId = assistito.CittadinanzaCod ?? 0,
            ComuneNascitaId = assistito.NascitaCodComune ?? 1,

            // ── Residenza ────────────────────────────────────────────────────
            ComuneResidenzaId = assistito.ResidenzaCodComune ?? 0,
            CapResidenza = assistito.ResidenzaCap,
            IndirizzoResidenza = assistito.ResidenzaIndirizzo,
            AreaResidenzaId = assistito.ResidenzaArea ?? 0,

            // ── Domicilio ────────────────────────────────────────────────────
            ComuneDomicilioId = assistito.DomicilioCodComune,
            CapDomicilio = assistito.DomicilioCap,
            IndirizzoDomicilio = assistito.DomicilioIndirizzo,
            AreaDomicilioId = assistito.DomicilioArea,

            // ── Reperibilità ────────────────────────────────────────────────
            ComuneReperibilitaId = assistito.ReperibilitaCodComune,
            CapReperibilita = assistito.ReperibilitaCap,
            IndirizzoReperibilita = assistito.ReperibilitaIndirizzo,
            NomeCampanelloReperibilita = assistito.ReperibilitaNomeCampanello,
            AreaReperibilitaId = assistito.ReperibilitaArea,

            // ── Contatti ─────────────────────────────────────────────────────
            Telefono1 = assistito.Telefono1,
            Telefono2 = assistito.Telefono2,
            Telefono3 = assistito.Telefono3,

            // ── Dati sociali/anagrafici (dai dati disponibili) ────────────────
            StatoCivileId = assistito.StatoCivileCod ?? 0,

            // ── Relazioni sanitarie ──────────────────────────────────────────
            MedicoId = assistito.MmgCod,

            // ── Campi non disponibili in VistaAssistito rimangono null/default
            // (Documento straniero, Dati sociali avanzati, Padre, Madre, Familiare, Codici alternativi, Note)

            // ── Audit (valorizzati correttamente) ────────────────────────────
            UtenteInserimento = currentUser.Id ?? 0,
            DataInserimento = DateTime.UtcNow,
            UtenteModifica = null,
            DataModifica = null,
            Version = 1,

            // ── Stato ────────────────────────────────────────────────────────
            Attivo = 1  // Nuovo paziente è attivo di default
        };

        await pazientiRepository.AddAsync(paziente, ct);
        await pazientiRepository.SaveChangesAsync(ct);

        return mapper.ToDto(paziente);
    }
}
