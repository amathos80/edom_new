/**
 * TIMEZONE HANDLING - FRONTEND BEST PRACTICES
 * ============================================
 * 
 * RULE: API sempre fornisce DateTime in UTC ISO 8601 (con "Z")
 * 
 * Esempio API Response:
 * {
 *   "id": 1,
 *   "cognome": "Rossi",
 *   "dataNascita": "1980-06-15",           // DateOnly: solo data
 *   "dataInserimento": "2026-05-09T10:30:00Z"  // DateTime: UTC con "Z"
 * }
 */

import { Injectable } from '@angular/core';
import { DatePipe } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class TimezoneService {
  constructor(private datePipe: DatePipe) {}

  /**
   * Converte ISO 8601 UTC string da API in Date object (locale automatico)
   * 
   * Input:  "2026-05-09T10:30:00Z" (UTC)
   * Output: Date object (JS automaticamente lo converte a locale Roma)
   *         quando lo stringifi = "Fri May 09 2026 12:30:00 GMT+0200 (Central European Summer Time)"
   */
  fromApiUtcString(isoString: string | null | undefined): Date | null {
    if (!isoString) return null;
    
    const date = new Date(isoString);
    
    // Validazione
    if (isNaN(date.getTime())) {
      console.warn(`Invalid ISO 8601 string: ${isoString}`);
      return null;
    }
    
    return date;
  }

  /**
   * Converte Date object locale in UTC ISO 8601 string per inviare API
   * 
   * Input:  Date object (Roma locale: 12:30)
   * Output: "2026-05-09T10:30:00Z" (UTC con offset -2h in estate)
   */
  toApiUtcString(date: Date | null | undefined): string | null {
    if (!date) return null;
    
    // toISOString() converte SEMPRE a UTC
    return date.toISOString();
  }

  /**
   * Formatta Date per visualizzazione locale nell'UI
   * 
   * Esempi di output (Roma):
   * - short:  "09/05/26, 12:30"
   * - long:   "9 May 2026 at 12:30:00"
   * - custom: "09 May 2026 12:30 (UTC+2)"
   */
  formatForDisplay(
    date: Date | null | undefined,
    format: string = 'short',
    timezone?: string
  ): string {
    if (!date) return '';
    
    // timezone default: browser locale
    // Per Roma: 'Europe/Rome' oppure null (usa sistema operativo)
    
    return this.datePipe.transform(date, format, timezone) || '';
  }

  /**
   * Converte Data senza ora (DateOnly da API) in Date object
   * 
   * Input:  "1980-06-15"
   * Output: Date object (00:00 locale)
   */
  fromApiDateOnlyString(dateOnlyString: string | null | undefined): Date | null {
    if (!dateOnlyString) return null;
    
    // Parse "YYYY-MM-DD"
    const match = dateOnlyString.match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (!match) {
      console.warn(`Invalid date-only format: ${dateOnlyString}`);
      return null;
    }

    const [, year, month, day] = match;
    // Crea Date: automaticamente in timezone locale, ore 00:00
    return new Date(parseInt(year), parseInt(month) - 1, parseInt(day));
  }

  /**
   * Converte Date in formato date-only per inviare API
   * 
   * Input:  Date object (qualunque ora)
   * Output: "YYYY-MM-DD"
   */
  toApiDateOnlyString(date: Date | null | undefined): string | null {
    if (!date) return null;
    
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    
    return `${year}-${month}-${day}`;
  }

  /**
   * Ottieni offset ora corrente per debug
   * 
   * Esempio output: "+02:00" (estate Roma)
   */
  getCurrentTimezoneOffset(): string {
    const now = new Date();
    const offset = -now.getTimezoneOffset(); // in minuti
    const hours = Math.floor(Math.abs(offset) / 60);
    const mins = Math.abs(offset) % 60;
    const sign = offset >= 0 ? '+' : '-';
    
    return `${sign}${String(hours).padStart(2, '0')}:${String(mins).padStart(2, '0')}`;
  }

  /**
   * DEBUG: Log info timezone per troubleshooting
   */
  logTimezoneInfo(): void {
    const now = new Date();
    const isoUtc = now.toISOString();
    const localString = now.toString();
    const offset = this.getCurrentTimezoneOffset();

    console.group('🌍 TIMEZONE INFO');
    console.log('Local Date String:', localString);
    console.log('ISO 8601 UTC:     ', isoUtc);
    console.log('Browser Offset:   ', offset);
    console.log('Locale:           ', Intl.DateTimeFormat().resolvedOptions().timeZone);
    console.groupEnd();
  }
}

// ════════════════════════════════════════════════════════════════════════════
// EXAMPLE USAGE IN COMPONENTS
// ════════════════════════════════════════════════════════════════════════════

/**
 * Esempio: Component che riceve datetime da API e lo visualizza
 */
export class PazienteDetailComponent {
  constructor(private tz: TimezoneService) {}

  paziente = {
    id: 1,
    cognome: 'Rossi',
    dataNascita: '1980-06-15',            // DateOnly string
    dataInserimento: '2026-05-09T10:30:00Z' // DateTime UTC
  };

  // Converti per template
  get dataNascitaDisplay(): Date | null {
    return this.tz.fromApiDateOnlyString(this.paziente.dataNascita);
  }

  get dataInserimentoDisplay(): Date | null {
    return this.tz.fromApiUtcString(this.paziente.dataInserimento);
  }

  // Nel template:
  // <div>
  //   Data Nascita: {{ dataNascitaDisplay | date:'longDate' }}
  //   Data Inserimento: {{ dataInserimentoDisplay | date:'short' }}
  // </div>

  // Invia aggiornamento (es. form POST)
  submitForm(formData: any): void {
    const payload = {
      ...formData,
      dataNascita: this.tz.toApiDateOnlyString(formData.dataNascitaInput),
      dataInserimento: this.tz.toApiUtcString(new Date()) // NOW in UTC
    };

    console.log('Inviando al server:', payload);
    // this.http.post('/api/pazienti', payload)
  }
}

// ════════════════════════════════════════════════════════════════════════════
// TEMPLATE USAGE
// ════════════════════════════════════════════════════════════════════════════

/**
 * Nel component template HTML:
 */

// 1. Visualizza DateTime da API (automaticamente locale)
// <p>Creato: {{ dataInserimento | date:'short' }}</p>
// OUTPUT: "09/05/26, 12:30" (se browser in Roma)

// 2. Visualizza DateOnly da API
// <p>Data Nascita: {{ dataNascita | date:'longDate' }}</p>
// OUTPUT: "June 15, 1980"

// 3. Input date picker (automaticamente locale)
// <input type="date" [ngModel]="dataNascita | date:'yyyy-MM-dd'" />
// NOTE: format di input HTML è sempre "YYYY-MM-DD" indipendente timezone

// 4. Input datetime picker
// <input type="datetime-local" [ngModel]="dataInserimento | date:'yyyy-MM-ddTHH:mm'" />
// NOTE: HTML datetime-local NON include offset, sarà locale

// ════════════════════════════════════════════════════════════════════════════
// COMMON MISTAKES TO AVOID
// ════════════════════════════════════════════════════════════════════════════

/**
 * ❌ SBAGLIATO - Assumptions di timezone:
 * 
 * new Date('2026-05-09 10:30:00')  // Ambiguo! Locale o UTC?
 * new Date(2026, 5, 9)              // Genera 2026-06-09 (mese 0-indexed!)
 * date.toLocaleString()             // Dipende da OS, non portable
 * JSON.stringify(date)              // Serializza come ISO ma perde timezone info
 */

/**
 * ✅ CORRETTO - Sempre esplicito:
 * 
 * const utc = new Date('2026-05-09T10:30:00Z')  // Esplicito UTC
 * const iso = utc.toISOString()                  // "2026-05-09T10:30:00Z"
 * const local = new Date(iso)                    // Browser converte a locale
 * const display = pipe.transform(local, 'short') // "09/05/26, 12:30"
 */

/**
 * ⚠️  ATTENZIONE - HTML date input:
 * 
 * <input type="date" value="2026-05-09" />
 * Questo è SEMPRE interpretato come local midnight (00:00)
 * NON include offset timezone!
 * 
 * Soluzione: usare formazione "yyyy-MM-dd" senza ora per DateOnly
 */
