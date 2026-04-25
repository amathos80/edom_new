using eDom.Core.Entities;
using Riok.Mapperly.Abstractions;

namespace eDom.Application.Features.Pazienti;

[Mapper]
public partial class PazientiMapper
{
    /// <summary>
    /// Paziente → PazienteDto.
    /// Usa la proprietà calcolata IsAttivo (bool) per popolare PazienteDto.Attivo.
    /// Il campo grezzo Attivo (short Oracle) e Telefono2/DataModifica vengono ignorati.
    /// </summary>
    [MapProperty(nameof(Paziente.IsAttivo), nameof(PazienteDto.Attivo))]
    [MapperIgnoreSource(nameof(Paziente.Attivo))]
    [MapperIgnoreSource(nameof(Paziente.Telefono2))]
    [MapperIgnoreSource(nameof(Paziente.DataModifica))]
    public partial PazienteDto ToDto(Paziente paziente);

    /// <summary>Lista — chiama ToDto per ogni elemento.</summary>
    public partial IEnumerable<PazienteDto> ToDtoList(IEnumerable<Paziente> pazienti);

    /// <summary>
    /// CreatePazienteCommand → nuova entità Paziente.
    /// Attivo, DataInserimento e tutti i campi non presenti nel command
    /// vanno impostati manualmente nel handler.
    /// </summary>
    [MapperIgnoreTarget(nameof(Paziente.Id))]
    [MapperIgnoreTarget(nameof(Paziente.Attivo))]
    [MapperIgnoreTarget(nameof(Paziente.DataInserimento))]
    [MapperIgnoreTarget(nameof(Paziente.DataModifica))]
    [MapperIgnoreTarget(nameof(Paziente.ValidFrom))]
    [MapperIgnoreTarget(nameof(Paziente.ValidTo))]
    [MapperIgnoreTarget(nameof(Paziente.CittadinanzaId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneNascitaId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaId))]
    [MapperIgnoreTarget(nameof(Paziente.AreaResidenzaId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneDomicilioId))]
    [MapperIgnoreTarget(nameof(Paziente.CapDomicilio))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoDomicilio))]
    [MapperIgnoreTarget(nameof(Paziente.AreaDomicilioId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneRepartoId))]
    [MapperIgnoreTarget(nameof(Paziente.CapReparto))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoReparto))]
    [MapperIgnoreTarget(nameof(Paziente.CameraReparto))]
    [MapperIgnoreTarget(nameof(Paziente.AreaRepartoId))]
    [MapperIgnoreTarget(nameof(Paziente.Telefono3))]
    [MapperIgnoreTarget(nameof(Paziente.TipoDocumentoStranieroId))]
    [MapperIgnoreTarget(nameof(Paziente.NumeroDocumentoStraniero))]
    [MapperIgnoreTarget(nameof(Paziente.ScadenzaDocumentoStraniero))]
    [MapperIgnoreTarget(nameof(Paziente.NumeroTeamEuropeo))]
    [MapperIgnoreTarget(nameof(Paziente.ScadenzaTeamEuropeo))]
    [MapperIgnoreTarget(nameof(Paziente.EsenzioneId))]
    [MapperIgnoreTarget(nameof(Paziente.DataEsenzione))]
    [MapperIgnoreTarget(nameof(Paziente.StatoCivileId))]
    [MapperIgnoreTarget(nameof(Paziente.TitoloStudioId))]
    [MapperIgnoreTarget(nameof(Paziente.ReligioneId))]
    [MapperIgnoreTarget(nameof(Paziente.EventoAnagrafId))]
    [MapperIgnoreTarget(nameof(Paziente.DataEventoAnagrafe))]
    [MapperIgnoreTarget(nameof(Paziente.ProfessioneId))]
    [MapperIgnoreTarget(nameof(Paziente.CondizioneProfId))]
    [MapperIgnoreTarget(nameof(Paziente.PosizioneProfId))]
    [MapperIgnoreTarget(nameof(Paziente.ConsultorioId))]
    [MapperIgnoreTarget(nameof(Paziente.CognomePadre))]
    [MapperIgnoreTarget(nameof(Paziente.NomePadre))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceFiscalePadre))]
    [MapperIgnoreTarget(nameof(Paziente.DataNascitaPadre))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoPadre))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaPadreId))]
    [MapperIgnoreTarget(nameof(Paziente.TelefonoPadre))]
    [MapperIgnoreTarget(nameof(Paziente.CognomeMadre))]
    [MapperIgnoreTarget(nameof(Paziente.NomeMadre))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceFiscaleMadre))]
    [MapperIgnoreTarget(nameof(Paziente.DataNascitaMadre))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoMadre))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaMadreId))]
    [MapperIgnoreTarget(nameof(Paziente.TelefonoMadre))]
    [MapperIgnoreTarget(nameof(Paziente.CognomeFam1))]
    [MapperIgnoreTarget(nameof(Paziente.NomeFam1))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceFiscaleFam1))]
    [MapperIgnoreTarget(nameof(Paziente.DataNascitaFam1))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoFam1))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaFam1Id))]
    [MapperIgnoreTarget(nameof(Paziente.TelefonoFam1))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceAlternativo1))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceAlternativo2))]
    [MapperIgnoreTarget(nameof(Paziente.Note))]
    [MapperIgnoreTarget(nameof(Paziente.UtenteInserimento))]
    [MapperIgnoreTarget(nameof(Paziente.UtenteModifica))]
    [MapperIgnoreTarget(nameof(Paziente.Version))]
    public partial Paziente FromCreate(CreatePazienteCommand command);

    /// <summary>
    /// Aggiorna un Paziente esistente con i dati dell'UpdatePazienteCommand.
    /// Id, Codice, DataInserimento e DataModifica non vengono toccati.
    /// Attivo (bool command) viene convertito in short Oracle tramite BoolToShort.
    /// </summary>
    [MapperIgnoreSource(nameof(UpdatePazienteCommand.Id))]
    [MapperIgnoreTarget(nameof(Paziente.Id))]
    [MapperIgnoreTarget(nameof(Paziente.Codice))]
    [MapperIgnoreTarget(nameof(Paziente.DataInserimento))]
    [MapperIgnoreTarget(nameof(Paziente.DataModifica))]
    [MapperIgnoreTarget(nameof(Paziente.ValidFrom))]
    [MapperIgnoreTarget(nameof(Paziente.ValidTo))]
    [MapperIgnoreTarget(nameof(Paziente.CittadinanzaId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneNascitaId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaId))]
    [MapperIgnoreTarget(nameof(Paziente.AreaResidenzaId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneDomicilioId))]
    [MapperIgnoreTarget(nameof(Paziente.CapDomicilio))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoDomicilio))]
    [MapperIgnoreTarget(nameof(Paziente.AreaDomicilioId))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneRepartoId))]
    [MapperIgnoreTarget(nameof(Paziente.CapReparto))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoReparto))]
    [MapperIgnoreTarget(nameof(Paziente.CameraReparto))]
    [MapperIgnoreTarget(nameof(Paziente.AreaRepartoId))]
    [MapperIgnoreTarget(nameof(Paziente.Telefono3))]
    [MapperIgnoreTarget(nameof(Paziente.TipoDocumentoStranieroId))]
    [MapperIgnoreTarget(nameof(Paziente.NumeroDocumentoStraniero))]
    [MapperIgnoreTarget(nameof(Paziente.ScadenzaDocumentoStraniero))]
    [MapperIgnoreTarget(nameof(Paziente.NumeroTeamEuropeo))]
    [MapperIgnoreTarget(nameof(Paziente.ScadenzaTeamEuropeo))]
    [MapperIgnoreTarget(nameof(Paziente.EsenzioneId))]
    [MapperIgnoreTarget(nameof(Paziente.DataEsenzione))]
    [MapperIgnoreTarget(nameof(Paziente.StatoCivileId))]
    [MapperIgnoreTarget(nameof(Paziente.TitoloStudioId))]
    [MapperIgnoreTarget(nameof(Paziente.ReligioneId))]
    [MapperIgnoreTarget(nameof(Paziente.EventoAnagrafId))]
    [MapperIgnoreTarget(nameof(Paziente.DataEventoAnagrafe))]
    [MapperIgnoreTarget(nameof(Paziente.ProfessioneId))]
    [MapperIgnoreTarget(nameof(Paziente.CondizioneProfId))]
    [MapperIgnoreTarget(nameof(Paziente.PosizioneProfId))]
    [MapperIgnoreTarget(nameof(Paziente.ConsultorioId))]
    [MapperIgnoreTarget(nameof(Paziente.CognomePadre))]
    [MapperIgnoreTarget(nameof(Paziente.NomePadre))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceFiscalePadre))]
    [MapperIgnoreTarget(nameof(Paziente.DataNascitaPadre))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoPadre))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaPadreId))]
    [MapperIgnoreTarget(nameof(Paziente.TelefonoPadre))]
    [MapperIgnoreTarget(nameof(Paziente.CognomeMadre))]
    [MapperIgnoreTarget(nameof(Paziente.NomeMadre))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceFiscaleMadre))]
    [MapperIgnoreTarget(nameof(Paziente.DataNascitaMadre))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoMadre))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaMadreId))]
    [MapperIgnoreTarget(nameof(Paziente.TelefonoMadre))]
    [MapperIgnoreTarget(nameof(Paziente.CognomeFam1))]
    [MapperIgnoreTarget(nameof(Paziente.NomeFam1))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceFiscaleFam1))]
    [MapperIgnoreTarget(nameof(Paziente.DataNascitaFam1))]
    [MapperIgnoreTarget(nameof(Paziente.IndirizzoFam1))]
    [MapperIgnoreTarget(nameof(Paziente.ComuneResidenzaFam1Id))]
    [MapperIgnoreTarget(nameof(Paziente.TelefonoFam1))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceAlternativo1))]
    [MapperIgnoreTarget(nameof(Paziente.CodiceAlternativo2))]
    [MapperIgnoreTarget(nameof(Paziente.Note))]
    [MapperIgnoreTarget(nameof(Paziente.UtenteInserimento))]
    [MapperIgnoreTarget(nameof(Paziente.UtenteModifica))]
    [MapperIgnoreTarget(nameof(Paziente.Version))]
    public partial void ApplyUpdate(UpdatePazienteCommand command, [MappingTarget] Paziente paziente);

    /// <summary>Conversione bool → short per il campo PAZI_F_ATT (Oracle NUMBER(1)).</summary>
    private static short BoolToShort(bool value) => value ? (short)1 : (short)0;
}
