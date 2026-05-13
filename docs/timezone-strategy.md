# Strategia Timezone e Migrazione Dati

## 1. Approccio Corretto (Best Practice)

### Database (PostgreSQL)
- **Standard**: Salvare sempre in **UTC** con tipo `TIMESTAMP WITH TIME ZONE` (o `TIMESTAMPTZ`)
- **Vantaggi**:
  - Indipendente dal server/session timezone
  - DST (ora legale) gestito automaticamente
  - Confronti cross-timezone corretti
  - Query internazionali senza ambiguità

```sql
-- Verifica colonna corrente (esempio CO_PAZIENTI)
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'CO_PAZIENTI' AND column_name LIKE '%DT%';
```

### API (.NET)
- **Serializzazione JSON**: Convertire a ISO 8601 con offset UTC
- **Input**: Accettare ISO 8601 (con o senza timezone, normalizzare a UTC)
- **Output**: Sempre rispondere in UTC come stringa ISO 8601

```csharp
// Configurazione Sistem.Text.Json in Program.cs
options.JsonSerializerOptions.Converters.Add(
    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

// DateTime sempre ISO 8601 UTC
DateTime utcNow = DateTime.UtcNow; // NON DateTime.Now
```

**Entity Configuration in EF Core**:
```csharp
// Aggiungere in OnModelCreating
modelBuilder.Entity<Paziente>(e =>
{
    // DateTime sempre UTC nel database
    e.Property(x => x.DataInserimento)
        .HasColumnType("timestamp with time zone")
        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
    
    e.Property(x => x.DataModifica)
        .HasColumnType("timestamp with time zone");
    
    // DateOnly per date senza ora
    e.Property(x => x.DataNascita)
        .HasColumnType("date");
});
```

### Frontend (Angular)
- **Ricezione**: Parse ISO 8601 string → `Date` object (automaticamente locale)
- **Visualizzazione**: Usare `date` pipe con timezone
- **Salvataggio**: Convertire locale a UTC ISO 8601 prima di inviare

```typescript
// Pipe display con timezone locale
{{ timestamp | date : 'short' : timezone }}

// Conversione before POST
const utcString = dateObj.toISOString(); // ISO 8601 UTC

// Conversione from API
const localDate = new Date(apiISOString); // Automaticamente locale
```

**Esempio completo**:
```typescript
// Ricevi da API (UTC ISO 8601)
const apiResponse = '2026-05-09T10:30:00Z'; // o '2026-05-09T10:30:00+00:00'

// Automaticamente convertito a locale nel browser
const localDate = new Date(apiResponse);
console.log(localDate.toString()); // Roma time (UTC+2 in estate)

// Invia indietro to API (sempre UTC)
const payload = {
  dataNascita: localDate.toISOString() // '2026-05-09T08:30:00.000Z'
};
```

---

## 2. Migrazione Dati Vecchi (Roma Locale → UTC)

### Fase 1: Audit - Identificare dati ambigui
```sql
-- Se il DB era timezone-naive, la roma locale è stata salvata come se fosse UTC
-- Esempio: data vera = 14:00 Roma (UTC+2 estate)
--          salvata come = 14:00 (senza offset)
--          interpretata ora come = 14:00 UTC

-- Controlla tipo colonna attuale
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_schema = 'HICT' 
  AND column_name IN ('PAZI_DTNAS', 'PAZI_DTINS', 'PAZI_DTMOD');
```

### Fase 2: Determina offset storico
- **UTC+1**: Novembre - Marzo (ora solare)
- **UTC+2**: Marzo - Ottobre (ora legale/CEST)
- **Regola DST Europa**: Ultima domenica di Marzo/Ottobre

```sql
-- Per data storica, calcola offset Roma
-- Esempio per 2024:
-- 1 gennaio 2024 → UTC+1
-- 15 giugno 2024 → UTC+2

-- Se colonne sono timezone-naive, applica conversione:
ALTER TABLE "HICT"."CO_PAZIENTI"
ALTER COLUMN "PAZI_DTINS" TYPE timestamp with time zone 
USING "PAZI_DTINS" AT TIME ZONE 'Europe/Rome';
```

### Fase 3: Script Migrazione (se necessario)

```sql
-- Backup PRIMA di modificare
-- STEP 1: Crea colonna temporanea
ALTER TABLE "HICT"."CO_PAZIENTI" 
ADD COLUMN "PAZI_DTINS_TMP" timestamp with time zone;

-- STEP 2: Converti Roma locale → UTC
-- Se la data è salvata come "14:00" (Roma locale, interpreted as UTC)
-- Sottrai offset Roma per ottenere UTC reale
UPDATE "HICT"."CO_PAZIENTI"
SET "PAZI_DTINS_TMP" = 
    "PAZI_DTINS" AT TIME ZONE 'Europe/Rome' AT TIME ZONE 'UTC'
WHERE "PAZI_DTINS" IS NOT NULL;

-- STEP 3: Verifica campione
SELECT "PAZI_DTINS", "PAZI_DTINS_TMP",
       extract(epoch FROM ("PAZI_DTINS_TMP" - "PAZI_DTINS")) / 3600 as hours_diff
FROM "HICT"."CO_PAZIENTI"
LIMIT 10;

-- STEP 4: Se corretto, sostituisci originale
ALTER TABLE "HICT"."CO_PAZIENTI" 
DROP COLUMN "PAZI_DTINS";

ALTER TABLE "HICT"."CO_PAZIENTI" 
RENAME COLUMN "PAZI_DTINS_TMP" TO "PAZI_DTINS";

-- STEP 5: Imposta default UTC
ALTER TABLE "HICT"."CO_PAZIENTI"
ALTER COLUMN "PAZI_DTINS" SET DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC');
```

### Fase 4: Verifica post-migrazione

```csharp
// Test API - Verifica che i timestamp sono UTC corretti
[HttpGet("test-timezone")]
public IActionResult TestTimezone()
{
    var paziente = _db.Pazienti.First();
    
    // Deve essere Kind = Utc
    if (paziente.DataInserimento.Kind != DateTimeKind.Utc)
    {
        return BadRequest("Data non è UTC!");
    }
    
    return Ok(new
    {
        dataInserimento = paziente.DataInserimento,
        isoString = paziente.DataInserimento.ToString("O"), // ISO 8601
        kind = paziente.DataInserimento.Kind
    });
}
```

---

## 3. Checklist Implementazione

### Database
- [ ] Verificare tipo colonne timestamp in CO_PAZIENTI e altri
- [ ] Se timezone-naive, applicare conversione `AT TIME ZONE 'Europe/Rome'`
- [ ] Impostare `TIMESTAMP WITH TIME ZONE` per nuove colonne
- [ ] Impostare default `CURRENT_TIMESTAMP AT TIME ZONE 'UTC'`

### API Backend (.NET)
- [ ] Configurare EF Core per UTC nelle colonne DateTime
- [ ] Verificare `DateTime.UtcNow` invece di `DateTime.Now` in audit fields
- [ ] Impostare JSON serializer per ISO 8601 UTC
- [ ] Test API: `/api/test-timezone` verifica Kind = Utc

### Frontend (Angular)
- [ ] Parse ISO 8601 automatico in `Date` object
- [ ] Display con `date` pipe (timezone locale automatico)
- [ ] Conversione to `toISOString()` prima di POST
- [ ] Test: Verifica "Ora Roma" vs "Ora UTC" in console

### Migrazione Dati
- [ ] Backup completo database
- [ ] Test script su replica DB first
- [ ] Applicare conversione `AT TIME ZONE 'Europe/Rome'` su tutte colonne data
- [ ] Verifica campione rows
- [ ] Rollback plan pronto

---

## 4. Riassunto Timeline Locale vs UTC

| Scenario | Vecchio (Roma Locale) | Nuovo (UTC) | Diff |
|----------|----------------------|------------|------|
| Inverno (dic) | 10:00 Roma UTC+1 | 09:00 UTC | -1h |
| Estate (lug) | 10:00 Roma UTC+2 | 08:00 UTC | -2h |
| Display Frontend | 10:00 (local) | 10:00 (converted) | Automatic |
| API JSON | "2026-05-09 10:00:00" | "2026-05-09T08:00:00Z" | Explicit |
| Database | No offset (ambiguo) | +00:00 (esplicito) | Clear |

---

## 5. Test Completo

```csharp
[TestMethod]
public void TestTimezoneMigration()
{
    // OLD: Era salvato come "2026-07-15 14:30:00" (Roma UTC+2)
    // NEW: Deve essere salvato come "2026-07-15 12:30:00Z" (UTC)
    
    var paziente = new Paziente 
    { 
        DataInserimento = DateTime.UtcNow,
        // ...
    };
    
    Assert.AreEqual(DateTimeKind.Utc, paziente.DataInserimento.Kind);
    
    var json = JsonSerializer.Serialize(paziente);
    Assert.IsTrue(json.Contains("Z")); // ISO 8601 UTC indicator
}
```

---

## Note Importanti

1. **DateOnly vs DateTime**:
   - `DateOnly`: Per campi senza ora (es. DataNascita) → tipo `date` PostgreSQL
   - `DateTime`: Per audit/timestamp → tipo `timestamp with time zone` PostgreSQL

2. **Timezone.Local**: ❌ EVITARE
   - Dipende dall'OS/server
   - Non portable
   - Causa bug in prod

3. **"Always UTC in Code"**:
   - `DateTime.UtcNow` ✅
   - `DateTime.Now` ❌
   - `.ToString("O")` per ISO 8601 ✅
