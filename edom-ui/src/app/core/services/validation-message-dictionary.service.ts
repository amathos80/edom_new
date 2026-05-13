import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

type SistemaMessaggioDto = {
  id: number;
  classe: string;
  nome: string;
  descrizione: string;
  lingua: string;
  custom01?: string | null;
  custom02?: string | null;
  custom03?: string | null;
  custom04?: string | null;
  custom05?: string | null;
  attivo: boolean;
};

@Injectable({ providedIn: 'root' })
export class ValidationMessageDictionaryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/sistema-messaggi`;
  private readonly dictionary = new Map<string, string>();
  private loaded = false;
  private loadingPromise: Promise<void> | null = null;

  private readonly fallbackMessages: Record<string, string> = {
    required: 'Campo obbligatorio.',
    minLength: 'Il campo deve contenere almeno {minLength} caratteri.',
    maxLength: 'Il campo non puo superare {maxLength} caratteri.',
    email: 'Inserisci un indirizzo email valido.',
    pattern: 'Il formato inserito non e valido.',
    number: 'Inserisci un valore numerico valido.',
    dateNotAllowedFuture: 'La data non puo essere successiva a oggi ({today}).',
    dateNotAllowedPast: 'La data non puo essere precedente a oggi ({today}).',
    dateBeforeMin: 'La data non puo essere precedente a {minDate}.',
    dateAfterMax: 'La data non puo essere successiva a {maxDate}.'
  };

  async loadMessages(): Promise<void> {
    if (this.loaded) {
      return;
    }

    if (this.loadingPromise) {
      return this.loadingPromise;
    }

    this.loadingPromise = this.loadMessagesInternal();
    try {
      await this.loadingPromise;
    } finally {
      this.loadingPromise = null;
    }
  }

  private async loadMessagesInternal(): Promise<void> {
    if (this.loaded) {
      return;
    }

    const lingua = this.resolveLanguage();
    const params = new HttpParams()
      .set('lingua', lingua)
      .set('soloAttivi', 'true');

    console.log('[ValidationMessageDictionary] Loading from:', this.baseUrl, 'with params:', {
      lingua,
      soloAttivi: 'true'
    });

    try {
      const items = await firstValueFrom(this.http.get<SistemaMessaggioDto[]>(this.baseUrl, { params }));
      this.dictionary.clear();
      
      console.log('[ValidationMessageDictionary] Received', items.length, 'items from API');
      
      for (const item of items) {
        this.dictionary.set(this.toLookupKey(item.classe, item.nome), item.descrizione);
      }
      this.loaded = true;
      
      // Debug: Log loaded messages
      console.log('[ValidationMessageDictionary] Messages loaded:', this.dictionary.size, 'entries');
      const entries = Array.from(this.dictionary.entries()).map(([key, value]) => ({ key, value }));
      console.table(entries);
    } catch (error) {
      // Keep fallbacks only when the endpoint is not available (e.g. unauthenticated startup).
      // Leave loaded=false so a later call can retry after authentication.
      console.error('[ValidationMessageDictionary] Failed to load messages:', error);
    }
  }

  getMessage(key: string, placeholders?: Record<string, unknown>): string {
    if (!this.loaded && !this.loadingPromise) {
      void this.loadMessages();
    }

    const normalizedKey = this.normalizeKey(key);
    const message = this.dictionary.get(normalizedKey);

    // Try fallback: extract just the message name part and look for generic validation message
    let fallback: string | undefined;
    const colonIndex = normalizedKey.indexOf(':');
    if (colonIndex > 0) {
      const messageName = normalizedKey.slice(colonIndex + 1);
      fallback = this.dictionary.get(`validation:${messageName}`);
    }

    // Extract plain message name for built-in fallback
    const plainName = colonIndex > 0 ? normalizedKey.slice(colonIndex + 1) : normalizedKey;
    const builtInFallback = this.fallbackMessages[plainName] ?? 'Valore non valido.';

    return this.interpolate(message ?? fallback ?? builtInFallback, placeholders);
  }

  private normalizeKey(key: string): string {
    return key.trim().toLowerCase();
  }

  private toLookupKey(messageClass: string, messageName: string): string {
    return `${messageClass.trim().toLowerCase()}:${messageName.trim().toLowerCase()}`;
  }

  private normalizeMessageName(messageName: string): string {
    const key = messageName.trim();
    if (!key) {
      return key;
    }

    const lower = key.toLowerCase();
    if (lower === 'maxlength') {
      return 'maxLength';
    }

    if (lower === 'minlength') {
      return 'minLength';
    }

    return key;
  }

  private resolveLanguage(): string {
    // Se vuoi usare la lingua del browser, decommenta le due righe seguenti:
    // if (typeof navigator !== 'undefined') {
    //   const lang = (navigator.language || 'it-IT')?.trim();
    //   return lang || 'it-IT';
    // }
    // Forza sempre l'italiano
    return 'it-IT';
  }

  private interpolate(template: string, placeholders?: Record<string, unknown>): string {
    if (!placeholders || Object.keys(placeholders).length === 0) {
      return template;
    }

    return template.replace(/\{(\w+)\}/g, (full, key: string) => {
      if (!(key in placeholders)) {
        return full;
      }

      const value = placeholders[key];
      return value == null ? '' : String(value);
    });
  }
}