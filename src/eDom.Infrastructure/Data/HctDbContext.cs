using eDom.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Data;

public class HctDbContext(DbContextOptions<HctDbContext> options) : DbContext(options)
{
    public DbSet<Utente> Utenti => Set<Utente>();
    public DbSet<SistemaMessaggio> SistemiMessaggi => Set<SistemaMessaggio>();
    public DbSet<Procedura> Procedure => Set<Procedura>();
    public DbSet<Funzione> Funzioni => Set<Funzione>();
    public DbSet<Ruolo> Ruoli => Set<Ruolo>();
    public DbSet<RuoloFunzione> RuoliFunzione => Set<RuoloFunzione>();
    public DbSet<Paziente> Pazienti => Set<Paziente>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LogAccesso> LogAccessi => Set<LogAccesso>();
    public DbSet<Configurazione> Configurazioni => Set<Configurazione>();
    public DbSet<UserDashboardLayout> UserDashboardLayouts => Set<UserDashboardLayout>();
    public DbSet<RefreshTokenSession> RefreshTokenSessions => Set<RefreshTokenSession>();
    public DbSet<UserTokenState> UserTokenStates => Set<UserTokenState>();

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
        modelBuilder.HasDefaultSchema("HICT");

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
                .UsingEntity<Dictionary<string, object>>(
                "SI_UTENRUOL",
                right => right.HasOne<Ruolo>()
                  .WithMany()
                                    .HasForeignKey("UTRU_RUOL_ID")
                  .HasPrincipalKey(r => r.Id),
                left  => left.HasOne<Utente>()
                 .WithMany()
                                 .HasForeignKey("UTRU_UTEN_ID")
                 .HasPrincipalKey(u => u.Id),
        join =>
        {
            join.ToTable("SI_UTENRUOL");
                        join.IndexerProperty<int>("UTRU_ID").HasColumnName("UTRU_ID").ValueGeneratedOnAdd();
                        join.IndexerProperty<int>("UTRU_UTEN_ID").HasColumnName("UTRU_UTEN_ID");
                        join.IndexerProperty<int>("UTRU_RUOL_ID").HasColumnName("UTRU_RUOL_ID");
                        join.IndexerProperty<int?>("UTRU_PROC_ID").HasColumnName("UTRU_PROC_ID");
            join.HasKey("UTRU_ID");
        });
        });

        // ── SI_RUOLI ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Ruolo>(e =>
        {
            e.ToTable("SI_RUOLI");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("RUOL_ID").UseIdentityAlwaysColumn();
            e.Property(x => x.ProcedureId).HasColumnName("RUOL_PROC_ID");
            e.Property(x => x.Codice).HasColumnName("RUOL_CODICE").HasMaxLength(50).IsRequired();
            e.Property(x => x.Descrizione).HasColumnName("RUOL_DESCR").HasMaxLength(200);
            e.Property(x => x.FlagAdmin).HasColumnName("RUOL_F_ADMIN");
            e.Property(x => x.UtenteInserimento).HasColumnName("RUOL_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("RUOL_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("RUOL_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("RUOL_DTMOD");
            e.Property(x => x.Version).HasColumnName("RUOL_VERSION");

            e.HasOne<Procedura>()
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

            // ── SI_SISMESS ─────────────────────────────────────────────────────────
            modelBuilder.Entity<SistemaMessaggio>(e =>
            {
                e.ToTable("SI_SISMESS");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("SISM_ID");
                e.Property(x => x.Classe).HasColumnName("SISM_CLASSE").HasMaxLength(50).IsRequired();
                e.Property(x => x.Nome).HasColumnName("SISM_NOME").HasMaxLength(50).IsRequired();
                e.Property(x => x.Descrizione).HasColumnName("SISM_DESCR").HasMaxLength(2000).IsRequired();
                e.Property(x => x.Lingua).HasColumnName("SISM_LINGUA").HasMaxLength(5).IsRequired();
                e.Property(x => x.Custom01).HasColumnName("SISM_CUSTOM01").HasMaxLength(255);
                e.Property(x => x.Custom02).HasColumnName("SISM_CUSTOM02").HasMaxLength(255);
                e.Property(x => x.Custom03).HasColumnName("SISM_CUSTOM03").HasMaxLength(255);
                e.Property(x => x.Custom04).HasColumnName("SISM_CUSTOM04").HasMaxLength(255);
                e.Property(x => x.Custom05).HasColumnName("SISM_CUSTOM05").HasMaxLength(255);
                e.Property(x => x.FlagAttivo).HasColumnName("SISM_F_ATTIVO");
                e.Property(x => x.UtenteInserimento).HasColumnName("SISM_UTINS");
                e.Property(x => x.DataInserimento).HasColumnName("SISM_DTINS");
                e.Property(x => x.UtenteModifica).HasColumnName("SISM_UTMOD");
                e.Property(x => x.DataModifica).HasColumnName("SISM_DTMOD");
                e.Property(x => x.Version).HasColumnName("SISM_VERSION");
            });

        // ── SI_PROCEDURE ──────────────────────────────────────────────────────
        modelBuilder.Entity<Procedura>(e =>
        {
            e.ToTable("SI_PROCEDURE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("PROC_ID");
            e.Property(x => x.Codice).HasColumnName("PROC_CODICE").HasMaxLength(50).IsRequired();
            e.Property(x => x.Descrizione).HasColumnName("PROC_DESCR").HasMaxLength(200).IsRequired();
            e.Property(x => x.UtenteInserimento).HasColumnName("PROC_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("PROC_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("PROC_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("PROC_DTMOD");
            e.Property(x => x.Version).HasColumnName("PROC_VERSION");
            e.Property(x => x.DbSchema).HasColumnName("PROC_DBSCHEMA").HasMaxLength(100);
            e.Property(x => x.DbPassword).HasColumnName("PROC_DBPWD").HasMaxLength(200);
            e.Property(x => x.Visibile).HasColumnName("PROC_VISIBILE");
        });

        // ── SI_FUNZIONI ───────────────────────────────────────────────────────
        modelBuilder.Entity<Funzione>(e =>
        {
            e.ToTable("SI_FUNZIONI");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("FUNZ_ID");
            e.Property(x => x.ProcedureId).HasColumnName("FUNZ_PROC_ID");
            e.Property(x => x.Codice).HasColumnName("FUNZ_CODICE").HasMaxLength(50).IsRequired();
            e.Property(x => x.Descrizione).HasColumnName("FUNZ_DESCR").HasMaxLength(200).IsRequired();
            e.Property(x => x.ParentId).HasColumnName("FUNZ_PARENT");
            e.Property(x => x.UtenteInserimento).HasColumnName("FUNZ_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("FUNZ_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("FUNZ_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("FUNZ_DTMOD");
            e.Property(x => x.Version).HasColumnName("FUNZ_VERSION");
            e.Property(x => x.Sort).HasColumnName("FUNZ_SORT").HasMaxLength(100);

            e.HasOne(x => x.Procedura)
                .WithMany(x => x.Funzioni)
                .HasForeignKey(x => x.ProcedureId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Padre)
                .WithMany(x => x.Figlie)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SI_RUOLFUNZ ───────────────────────────────────────────────────────
        modelBuilder.Entity<RuoloFunzione>(e =>
        {
            e.ToTable("SI_RUOLFUNZ");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("RUFU_ID").UseIdentityAlwaysColumn();
            e.Property(x => x.RuoloId).HasColumnName("RUFU_RUOL_ID");
            e.Property(x => x.FunzioneId).HasColumnName("RUFU_FUNZ_ID");
            e.Property(x => x.RuoloProcedureId).HasColumnName("RUFU_RUOL_PROC_ID");
            e.Property(x => x.FunzioneProcedureId).HasColumnName("RUFU_FUNZ_PROC_ID");
            e.Property(x => x.UtenteInserimento).HasColumnName("RUFU_UTINS");
            e.Property(x => x.DataInserimento).HasColumnName("RUFU_DTINS");
            e.Property(x => x.UtenteModifica).HasColumnName("RUFU_UTMOD");
            e.Property(x => x.DataModifica).HasColumnName("RUFU_DTMOD");
            e.Property(x => x.Version).HasColumnName("RUFU_VERSION");

            e.HasOne(x => x.Ruolo)
                .WithMany()
                .HasForeignKey(x => x.RuoloId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Funzione)
                .WithMany(x => x.RuoliFunzione)
                .HasForeignKey(x => x.FunzioneId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.RuoloProcedura)
                .WithMany()
                .HasForeignKey(x => x.RuoloProcedureId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.FunzioneProcedura)
                .WithMany()
                .HasForeignKey(x => x.FunzioneProcedureId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── CO_PAZIENTI ────────────────────────────────────────────────────────
        modelBuilder.Entity<Paziente>(e =>
        {
            e.ToTable("CO_PAZIENTI");
            e.HasKey(x => x.Id);

            // Identificazione
            e.Property(x => x.Id).HasColumnName("PAZI_ID").UseIdentityAlwaysColumn();
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
            e.Property(x => x.Id).HasColumnName("AULO_ID").UseIdentityAlwaysColumn();
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
            e.Property(x => x.Id).HasColumnName("LOAC_ID").UseIdentityAlwaysColumn();
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

        // ── APP_DASH_LAYOUT ────────────────────────────────────────────────────
        modelBuilder.Entity<UserDashboardLayout>(e =>
        {
            e.ToTable("APP_DASH_LAYOUT");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("DASH_ID").UseIdentityAlwaysColumn();
            e.Property(x => x.UserCodice).HasColumnName("DASH_USER_CODICE").HasMaxLength(100).IsRequired();
            e.Property(x => x.LayoutJson).HasColumnName("DASH_LAYOUT_JSON").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("DASH_UPDATED_AT").IsRequired();
            e.HasIndex(x => x.UserCodice).IsUnique().HasDatabaseName("UX_DASH_USER_CODICE");
        });

        // ── APP_REFRESH_TOKENS ───────────────────────────────────────────────
        modelBuilder.Entity<RefreshTokenSession>(e =>
        {
            e.ToTable("APP_REFRESH_TOKENS");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("RFTK_ID");
            e.Property(x => x.UserId).HasColumnName("RFTK_UTEN_ID");
            e.Property(x => x.TokenHash).HasColumnName("RFTK_TOKEN_HASH").HasMaxLength(128).IsRequired();
            e.Property(x => x.FamilyId).HasColumnName("RFTK_FAMILY_ID");
            e.Property(x => x.CreatedAtUtc).HasColumnName("RFTK_CREATED_UTC");
            e.Property(x => x.ExpiresAtUtc).HasColumnName("RFTK_EXPIRES_UTC");
            e.Property(x => x.RevokedAtUtc).HasColumnName("RFTK_REVOKED_UTC");
            e.Property(x => x.RevokedReason).HasColumnName("RFTK_REVOKED_REASON").HasMaxLength(200);
            e.Property(x => x.CreatedByIp).HasColumnName("RFTK_CREATED_IP").HasMaxLength(64);
            e.Property(x => x.RevokedByIp).HasColumnName("RFTK_REVOKED_IP").HasMaxLength(64);
            e.Property(x => x.ReplacedByTokenId).HasColumnName("RFTK_REPLACED_BY_ID");

            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.FamilyId);
            e.HasIndex(x => x.ExpiresAtUtc);
        });

        // ── APP_USER_TOKEN_STATE ─────────────────────────────────────────────
        modelBuilder.Entity<UserTokenState>(e =>
        {
            e.ToTable("APP_USER_TOKEN_STATE");
            e.HasKey(x => x.UserId);

            e.Property(x => x.UserId).HasColumnName("UTST_UTEN_ID");
            e.Property(x => x.InvalidBeforeUtc).HasColumnName("UTST_INVALID_BEFORE_UTC");
            e.Property(x => x.UpdatedAtUtc).HasColumnName("UTST_UPDATED_UTC");
        });
    }
}
