using eDom.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Data;

public class HctDbContext(DbContextOptions<HctDbContext> options) : DbContext(options)
{
    public DbSet<Utente> Utenti => Set<Utente>();
    public DbSet<Ruolo> Ruoli => Set<Ruolo>();
    public DbSet<Paziente> Pazienti => Set<Paziente>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LogAccesso> LogAccessi => Set<LogAccesso>();
    public DbSet<Configurazione> Configurazioni => Set<Configurazione>();

    /// <summary>
    /// Quando true, AuditInterceptor non genera record per questo contesto.
    /// Usare SuppressAudit() / ResumeAudit() nelle operazioni bulk.
    /// </summary>
    internal bool AuditSuppressed { get; private set; }

    public void SuppressAudit() => AuditSuppressed = true;
    public void ResumeAudit()   => AuditSuppressed = false;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── SI_UTENTI ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Utente>(e =>
        {
            e.ToTable("SI_UTENTI");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("UTEN_ID");
            e.Property(x => x.Codice).HasColumnName("UTEN_CODICE").HasMaxLength(50).IsRequired();
            e.Property(x => x.Password).HasColumnName("UTEN_PASSWD").HasMaxLength(200).IsRequired();
            e.Property(x => x.Cognome).HasColumnName("UTEN_COGNOME").HasMaxLength(100);
            e.Property(x => x.Nome).HasColumnName("UTEN_NOME").HasMaxLength(100);
            e.Property(x => x.CodiceFiscale).HasColumnName("UTEN_CODFISC").HasMaxLength(20);
            e.Property(x => x.Email).HasColumnName("UTEN_EMAIL").HasMaxLength(200);
            e.Property(x => x.Matricola).HasColumnName("UTEN_MATR").HasMaxLength(50);
            e.Property(x => x.FlagCambiaPwd).HasColumnName("UTEN_F_CHPASS");
            e.Property(x => x.FlagSmartCard).HasColumnName("UTEN_F_SMCARD");
            e.Property(x => x.DataDisattivazione).HasColumnName("UTEN_DTDISAT");
            e.Property(x => x.DataRiattivazione).HasColumnName("UTEN_DTRIAT");
            e.Property(x => x.DataScadenzaPassword).HasColumnName("UTEN_DTSCPASS");
            e.Property(x => x.UltimoLogin).HasColumnName("UTEN_LASTLOGIN");
            e.Property(x => x.UtenteInserimento).HasColumnName("UTEN_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("UTEN_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("UTEN_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("UTEN_DTMOD");
            e.Property(x => x.Version).HasColumnName("UTEN_VERSION");
            e.Ignore(x => x.NomeCompleto);
            e.Ignore(x => x.IsAttivo);
            e.HasMany(x => x.Ruoli).WithMany(r => r.Utenti)
                .UsingEntity(j => j.ToTable("SI_UTENRUOL"));
        });

        // ── SI_RUOLI ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Ruolo>(e =>
        {
            e.ToTable("SI_RUOLI");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("RUOL_ID");
            e.Property(x => x.ProcedureId).HasColumnName("RUOL_PROC_ID");
            e.Property(x => x.Codice).HasColumnName("RUOL_CODICE").HasMaxLength(50).IsRequired();
            e.Property(x => x.Descrizione).HasColumnName("RUOL_DESCR").HasMaxLength(200);
            e.Property(x => x.FlagAdmin).HasColumnName("RUOL_F_ADMIN");
            e.Property(x => x.UtenteInserimento).HasColumnName("RUOL_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("RUOL_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("RUOL_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("RUOL_DTMOD");
            e.Property(x => x.Version).HasColumnName("RUOL_VERSION");
        });

        // ── CO_PAZIENTI ────────────────────────────────────────────────────────
        modelBuilder.Entity<Paziente>(e =>
        {
            e.ToTable("CO_PAZIENTI");
            e.HasKey(x => x.Id);

            // Identificazione
            e.Property(x => x.Id).HasColumnName("PAZI_ID");
            e.Property(x => x.Codice).HasColumnName("PAZI_CODICE").HasMaxLength(50).IsRequired();
            e.Property(x => x.ValidFrom).HasColumnName("PAZI_VALID_FROM");
            e.Property(x => x.ValidTo).HasColumnName("PAZI_VALID_TO");

            // Anagrafica base
            e.Property(x => x.Cognome).HasColumnName("PAZI_COGNOME").HasMaxLength(100).IsRequired();
            e.Property(x => x.Nome).HasColumnName("PAZI_NOME").HasMaxLength(100).IsRequired();
            e.Property(x => x.DataNascita).HasColumnName("PAZI_DTNAS");
            e.Property(x => x.CodiceFiscale).HasColumnName("PAZI_CODFISC").HasMaxLength(20).IsRequired();
            e.Property(x => x.Sesso).HasColumnName("PAZI_SESSO").HasMaxLength(1).IsRequired();
            e.Property(x => x.Email).HasColumnName("PAZI_EMAIL").HasMaxLength(200);
            e.Property(x => x.CodiceSanitario).HasColumnName("PAZI_CODICESANIT").HasMaxLength(50).IsRequired();
            e.Property(x => x.CittadinanzaId).HasColumnName("PAZI_CITTADIN_ID");
            e.Property(x => x.ComuneNascitaId).HasColumnName("PAZI_COMNAS_ID");

            // Residenza
            e.Property(x => x.ComuneResidenzaId).HasColumnName("PAZI_COMRES_ID");
            e.Property(x => x.CapResidenza).HasColumnName("PAZI_CAPRES").HasMaxLength(10);
            e.Property(x => x.IndirizzoResidenza).HasColumnName("PAZI_INDRES").HasMaxLength(200);
            e.Property(x => x.AreaResidenzaId).HasColumnName("PAZI_AREARES_ID");

            // Domicilio
            e.Property(x => x.ComuneDomicilioId).HasColumnName("PAZI_COMDOM_ID");
            e.Property(x => x.CapDomicilio).HasColumnName("PAZI_CAPDOM").HasMaxLength(10);
            e.Property(x => x.IndirizzoDomicilio).HasColumnName("PAZI_INDDOM").HasMaxLength(200);
            e.Property(x => x.AreaDomicilioId).HasColumnName("PAZI_AREADOM_ID");

            // Reparto
            e.Property(x => x.ComuneRepartoId).HasColumnName("PAZI_COMREP_ID");
            e.Property(x => x.CapReparto).HasColumnName("PAZI_CAPREP").HasMaxLength(10);
            e.Property(x => x.IndirizzoReparto).HasColumnName("PAZI_INDREP").HasMaxLength(200);
            e.Property(x => x.CameraReparto).HasColumnName("PAZI_CAMPREP").HasMaxLength(50);
            e.Property(x => x.AreaRepartoId).HasColumnName("PAZI_AREAREP_ID");

            // Contatti
            e.Property(x => x.Telefono1).HasColumnName("PAZI_TELEF01").HasMaxLength(30);
            e.Property(x => x.Telefono2).HasColumnName("PAZI_TELEF02").HasMaxLength(30);
            e.Property(x => x.Telefono3).HasColumnName("PAZI_TELEF03").HasMaxLength(30);

            // Documento straniero
            e.Property(x => x.TipoDocumentoStranieroId).HasColumnName("PAZI_ELTG_STRANDOCTIPO");
            e.Property(x => x.NumeroDocumentoStraniero).HasColumnName("PAZI_STRANDOCNUM").HasMaxLength(50);
            e.Property(x => x.ScadenzaDocumentoStraniero).HasColumnName("PAZI_STRANDOCSCAD");
            e.Property(x => x.NumeroTeamEuropeo).HasColumnName("PAZI_TEAMNUM").HasMaxLength(50);
            e.Property(x => x.ScadenzaTeamEuropeo).HasColumnName("PAZI_TEAMSCAD");

            // Dati sociali
            e.Property(x => x.EsenzioneId).HasColumnName("PAZI_ESENZ_ID");
            e.Property(x => x.DataEsenzione).HasColumnName("PAZI_DTESENZ");
            e.Property(x => x.StatoCivileId).HasColumnName("PAZI_ELTG_STATOCIV");
            e.Property(x => x.TitoloStudioId).HasColumnName("PAZI_ELTG_TITSTU");
            e.Property(x => x.ReligioneId).HasColumnName("PAZI_ELTG_RELIG");
            e.Property(x => x.EventoAnagrafId).HasColumnName("PAZI_ELTG_EVEANAG");
            e.Property(x => x.DataEventoAnagrafe).HasColumnName("PAZI_DTEVEANAG");
            e.Property(x => x.ProfessioneId).HasColumnName("PAZI_ELTG_PROFES");
            e.Property(x => x.CondizioneProfId).HasColumnName("PAZI_ELTG_CONDPROFES");
            e.Property(x => x.PosizioneProfId).HasColumnName("PAZI_ELTG_POSPROFES");

            // Relazioni sanitarie
            e.Property(x => x.MedicoId).HasColumnName("PAZI_MEDICO_ID");
            e.Property(x => x.ConsultorioId).HasColumnName("PAZI_CONSULTORIO_ID");

            // Padre
            e.Property(x => x.CognomePadre).HasColumnName("PAZI_COGNOMEPADRE").HasMaxLength(100);
            e.Property(x => x.NomePadre).HasColumnName("PAZI_NOMEPADRE").HasMaxLength(100);
            e.Property(x => x.CodiceFiscalePadre).HasColumnName("PAZI_CODFISCPADRE").HasMaxLength(20);
            e.Property(x => x.DataNascitaPadre).HasColumnName("PAZI_DTNASPADRE");
            e.Property(x => x.IndirizzoPadre).HasColumnName("PAZI_INDRESPADRE").HasMaxLength(200);
            e.Property(x => x.ComuneResidenzaPadreId).HasColumnName("PAZI_COMRESPADRE_ID");
            e.Property(x => x.TelefonoPadre).HasColumnName("PAZI_TELEFPADRE").HasMaxLength(30);

            // Madre
            e.Property(x => x.CognomeMadre).HasColumnName("PAZI_COGNOMEMADRE").HasMaxLength(100);
            e.Property(x => x.NomeMadre).HasColumnName("PAZI_NOMEMADRE").HasMaxLength(100);
            e.Property(x => x.CodiceFiscaleMadre).HasColumnName("PAZI_CODFISCMADRE").HasMaxLength(20);
            e.Property(x => x.DataNascitaMadre).HasColumnName("PAZI_DTNASMADRE");
            e.Property(x => x.IndirizzoMadre).HasColumnName("PAZI_INDRESMADRE").HasMaxLength(200);
            e.Property(x => x.ComuneResidenzaMadreId).HasColumnName("PAZI_COMRESMADRE_ID");
            e.Property(x => x.TelefonoMadre).HasColumnName("PAZI_TELEFMADRE").HasMaxLength(30);

            // Familiare 1
            e.Property(x => x.CognomeFam1).HasColumnName("PAZI_COGNOMEFAM1").HasMaxLength(100);
            e.Property(x => x.NomeFam1).HasColumnName("PAZI_NOMEFAM1").HasMaxLength(100);
            e.Property(x => x.CodiceFiscaleFam1).HasColumnName("PAZI_CODFISCFAM1").HasMaxLength(20);
            e.Property(x => x.DataNascitaFam1).HasColumnName("PAZI_DTNASFAM1");
            e.Property(x => x.IndirizzoFam1).HasColumnName("PAZI_INDRESFAM1").HasMaxLength(200);
            e.Property(x => x.ComuneResidenzaFam1Id).HasColumnName("PAZI_COMRESFAM1_ID");
            e.Property(x => x.TelefonoFam1).HasColumnName("PAZI_TELEFFAM1").HasMaxLength(30);

            // Campi alternativi / note
            e.Property(x => x.CodiceAlternativo1).HasColumnName("PAZI_CODALT01").HasMaxLength(50);
            e.Property(x => x.CodiceAlternativo2).HasColumnName("PAZI_CODALT02").HasMaxLength(50);
            e.Property(x => x.Note).HasColumnName("PAZI_NOTE01").HasMaxLength(2000);

            // Audit
            e.Property(x => x.UtenteInserimento).HasColumnName("PAZI_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("PAZI_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("PAZI_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("PAZI_DTMOD");
            e.Property(x => x.Version).HasColumnName("PAZI_VERSION");

            // Stato
            e.Property(x => x.Attivo).HasColumnName("PAZI_F_ATT");

            // Computed (non persistiti)
            e.Ignore(x => x.NomeCompleto);
            e.Ignore(x => x.IsAttivo);
        });

        // ── SI_AUDIT_LOG ───────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("SI_AUDIT_LOG");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("AULO_ID").ValueGeneratedOnAdd();
            e.Property(x => x.Tabella).HasColumnName("AULO_TABELLA").HasMaxLength(100).IsRequired();
            e.Property(x => x.EntitaId).HasColumnName("AULO_ENTITA_ID").HasMaxLength(50).IsRequired();
            e.Property(x => x.Operazione).HasColumnName("AULO_OPERAZIONE").HasMaxLength(10).IsRequired();
            e.Property(x => x.UtenteId).HasColumnName("AULO_UTEN_ID");
            e.Property(x => x.DataOperazione).HasColumnName("AULO_DTOP").IsRequired();
            e.Property(x => x.ValoriPrecedenti).HasColumnName("AULO_OLD_VALUES");
            e.Property(x => x.ValoriNuovi).HasColumnName("AULO_NEW_VALUES");
        });

        // ── SI_LOGACC ──────────────────────────────────────────────────────────
        modelBuilder.Entity<LogAccesso>(e =>
        {
            e.ToTable("SI_LOGACC");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("LOAC_ID").ValueGeneratedOnAdd();
            e.Property(x => x.UtenteId).HasColumnName("LOAC_UTEN_ID");
            e.Property(x => x.Data).HasColumnName("LOAC_DATE").IsRequired();
            e.Property(x => x.IndirizzoIp).HasColumnName("LOAC_IPADDR").HasMaxLength(50);
            e.Property(x => x.NomeMacchina).HasColumnName("LOAC_MACHINE").HasMaxLength(200);
            e.Property(x => x.ProcedureId).HasColumnName("LOAC_PROC_ID");
            e.Property(x => x.FunzioneId).HasColumnName("LOAC_FUNZ_ID");
        });

        // ── SI_CONFIG ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Configurazione>(e =>
        {
            e.ToTable("SI_CONFIG");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("CONF_ID");
            e.Property(x => x.ProcedureId).HasColumnName("CONF_PROC_ID");
            e.Property(x => x.Codice).HasColumnName("CONF_CODICE").HasMaxLength(100).IsRequired();
            e.Property(x => x.Valore).HasColumnName("CONF_VALORE").HasMaxLength(500);
            e.Property(x => x.Descrizione).HasColumnName("CONF_DESCR").HasMaxLength(500);
        });
    }
}
