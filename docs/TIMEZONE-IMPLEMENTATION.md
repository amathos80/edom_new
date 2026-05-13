# Implementazione Timezone - Checklist Pratica

## 📋 Situazione Attuale
- **Vecchia app**: Salvava in Roma locale (timezone-naive = ambiguo)
- **Nuova app**: Deve usare UTC esplicito (standard internazionale)
- **Problema**: Migrazione dati senza perdere traccia delle date originali

---

## 🎯 Approccio Consigliato (Migliore)

### 1️⃣ Database (PostgreSQL)

**Cosa fare ADESSO** (non necessita cambio schema):
```sql
-- Verifica stato corrente
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_schema = 'HICT' 
  AND table_name = 'CO_PAZIENTI'
  AND column_name IN ('PAZI_DTINS', 'PAZI_DTMOD', 'PAZI_DTNAS');
```

**Risultati possibili**:
- Se `timestamp with time zone` → ✅ OK, già UTC-aware
- Se `timestamp without time zone` → ⚠️ Ambiguo, fare migrazione
- Se `date` → ✅ OK per date senza ora

**Migrazione** (una sola volta, eseguire in manutenzione):
```sql
-- File: /docs/sql/timezone-migration.sql
-- Convertire da Roma locale a UTC:
-- "14:00" (salvato) → intendeva 14:00 Roma (UTC+2)
-- Converti a: "12:00" (UTC)

-- Comando chiave:
UPDATE "HICT"."CO_PAZIENTI"
SET "PAZI_DTINS" = "PAZI_DTINS" AT TIME ZONE 'Europe/Rome' AT TIME ZONE 'UTC'
WHERE "PAZI_DTINS" IS NOT NULL;
```

**Dopo migrazione**:
```sql
-- Imposta default future values in UTC
ALTER TABLE "HICT"."CO_PAZIENTI"
ALTER COLUMN "PAZI_DTINS" SET DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC');
```

---

### 2️⃣ API Backend (.NET)

**Cosa fare ADESSO**:

1. Configura JSON serialization in `Program.cs`:
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions
            .ConfigureUtcDateTime(); // Extension method
    });
```

2. **IMPORTANTE**: Usa `DateTime.UtcNow` SEMPRE (non `DateTime.Now`):
```csharp
// ✅ CORRETTO
var paziente = new Paziente 
{
    DataInserimento = DateTime.UtcNow,  // UTC
    DataModifica = DateTime.UtcNow      // UTC
};

// ❌ SBAGLIATO
var paziente = new Paziente 
{
    DataInserimento = DateTime.Now,     // Local! (dipende da timezone OS)
};
```

3. **Entity Configuration** in `HctDbContext.cs` (aggiungere):
```csharp
modelBuilder.Entity<Paziente>(e =>
{
    // DateTime fields → timestamp with time zone (UTC)
    e.Property(x => x.DataInserimento)
        .HasColumnType("timestamp with time zone")
        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
    
    e.Property(x => x.DataModifica)
        .HasColumnType("timestamp with time zone");
    
    // DateOnly fields → date (senza ora)
    e.Property(x => x.DataNascita)
        .HasColumnType("date");
});
```

4. **JSON Response** - Automatico!
   - DateTime UTC come `"2026-05-09T10:30:00Z"` ✅
   - Pipe Angular automaticamente lo legge come UTC

---

### 3️⃣ Frontend (Angular)

**Cosa fare ADESSO**:

1. Usa `TimezoneService` (già creato):
```typescript
import { TimezoneService } from '@core/services/timezone.service';

export class PazienteDetailComponent {
  constructor(private tz: TimezoneService) {}

  // Ricevi da API
  paziente = {
    dataInserimento: '2026-05-09T10:30:00Z'  // UTC ISO 8601
  };

  // Converti in locale per template
  get dataInserimentoDisplay(): Date | null {
    return this.tz.fromApiUtcString(this.paziente.dataInserimento);
  }
}
```

2. Template HTML (semplicissimo):
```html
<!-- Visualizza automaticamente in ora locale Roma -->
<p>Creato: {{ dataInserimentoDisplay | date:'short' }}</p>
<!-- OUTPUT: "09/05/26, 12:30" se in Roma (UTC+2) -->

<!-- Per inviare indietro al server -->
<button (click)="submitForm()">Salva</button>
```

3. Invia indietro in UTC:
```typescript
submitForm(): void {
  const now = new Date();  // Local
  const payload = {
    dataInserimento: this.tz.toApiUtcString(now)  // Converti a UTC ISO 8601
  };
  // payload = { dataInserimento: "2026-05-09T10:30:00Z" } ✅
  this.http.post('/api/pazienti', payload);
}
```

---

## 🚀 Roadmap Implementazione

### Fase 1: Preparazione (SUBITO - 30 min)
- [x] Documentazione creata (`timezone-strategy.md`)
- [x] Script SQL diagnostico (`timezone-diagnostics.sql`)
- [x] Script migrazione (`timezone-migration.sql`)
- [x] Configurazione API (`TimezoneJsonConfiguration.cs`)
- [x] Service Frontend (`timezone.service.ts`)

**TODO**:
- [ ] Eseguire `timezone-diagnostics.sql` per verificare stato DB
- [ ] Aggiornare `Program.cs` per aggiungere `ConfigureUtcDateTime()`
- [ ] Verificare che tutti gli `DateTime.Now` siano `DateTime.UtcNow`

### Fase 2: Migrazione Dati (PIANIFICATA - quando possibile)
- [ ] Backup completo database
- [ ] Test migrazione su replica
- [ ] Eseguire `timezone-migration.sql`
- [ ] Verifica campione: `timezone-diagnostics.sql` post-migrazione
- [ ] Build + deploy nuova versione API

### Fase 3: Frontend (INCREMENTALE)
- [ ] Importare `TimezoneService` nei componenti
- [ ] Convertire datetime da API con `.fromApiUtcString()`
- [ ] Update template pipes
- [ ] Test locale Roma vs UTC nel console

---

## 🧪 Test / Verifica

### Test API (localhost:5075)
```bash
# Verifica che DateTime è UTC in risposta
curl http://localhost:5075/api/pazienti/1 | jq .dataInserimento

# Output atteso: "2026-05-09T10:30:00Z" (con Z = UTC)
# Output sbagliato: "2026-05-09T10:30:00" (senza offset = ambiguo!)
```

### Test Frontend (localhost:4200)
```typescript
// In browser console
const ts = new TimezoneService();
ts.logTimezoneInfo();

// Output:
// Local Date String: Fri May 09 2026 12:30:00 GMT+0200 (Central European Summer Time)
// ISO 8601 UTC:      2026-05-09T10:30:00.000Z
// Browser Offset:    +02:00
// Locale:            Europe/Rome
```

### Test Database
```sql
-- Verifica offset salvato
SELECT 
    pazi_id,
    pazi_dtins,
    EXTRACT(HOUR FROM pazi_dtins AT TIME ZONE 'UTC') as hour_utc,
    EXTRACT(HOUR FROM pazi_dtins AT TIME ZONE 'Europe/Rome') as hour_roma
FROM "HICT"."CO_PAZIENTI"
LIMIT 1;

-- OUTPUT atteso (dopo migrazione):
-- hour_utc = 10, hour_roma = 12 (differenza 2 ore in estate)
```

---

## 📚 Regole Ricordare Sempre

### ✅ SEMPRE:
1. **Database**: `timestamp with time zone` per audit
2. **Code**: `DateTime.UtcNow` (mai `DateTime.Now`)
3. **JSON**: ISO 8601 con "Z" suffix per UTC
4. **Frontend**: Parse `toISOString()` ricevendo, inviare `toISOString()` mandando

### ❌ MAI:
1. `DateTime.Now` nel codice backend
2. Timezone-naive DateTime (Unspecified kind)
3. Assumptions su timezone dell'OS/server
4. "Salviamo in Roma locale" senza offset

---

## 🆘 Troubleshooting

| Sintomo | Causa | Soluzione |
|---------|-------|-----------|
| "12:30 nel DB ma voglio 14:30" | Timezone offset off by 2h | Migrazione non corretta? Controlla `AT TIME ZONE 'Europe/Rome'` |
| Frontend mostra ora sbagliata | DateTime ricevuto non è UTC | API non serializza in UTC, check JSON converter |
| "DateTime.Kind = Unspecified" | Code usando `DateTime.Now` | Find & replace `DateTime.Now` → `DateTime.UtcNow` |
| Differenza 1h (DST) | Daylight Saving Time non gestito | PostgreSQL gestisce automatico con `AT TIME ZONE 'Europe/Rome'` |

---

## 📞 Riferimenti Rapidi

- **Best Practice**: [timezone-strategy.md](timezone-strategy.md)
- **SQL Diagnostica**: [timezone-diagnostics.sql](sql/timezone-diagnostics.sql)
- **SQL Migrazione**: [timezone-migration.sql](sql/timezone-migration.sql)
- **API Config**: [TimezoneJsonConfiguration.cs](../src/eDom.Api/Configuration/TimezoneJsonConfiguration.cs)
- **Frontend Service**: [timezone.service.ts](../edom-ui/src/app/core/services/timezone.service.ts)

---

## 🎓 Lezione Chiave

**Prima**: "14:00" salvato nel DB → ambiguo! (Roma o UTC?)
**Dopo**: "14:00" = "2026-05-09T10:30:00Z" (chiaro: Roma 12:30 UTC)

- Database sempre UTC esplicito
- Backend sempre UTC
- Frontend sempre locale (automatic)
- Migrazione one-time per dati storici

✅ **Easy, scalabile, internazionale!**
