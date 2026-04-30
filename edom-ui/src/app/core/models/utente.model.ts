export interface Utente {
  id: number;
  codice: string;
  cognome: string;
  nome: string;
  codiceFiscale?: string | null;
  email?: string | null;
  matricola?: string | null;
  flagSmartCard: boolean;
  flagCambiaPwd: boolean;
  dataDisattivazione?: string | null;
  dataRiattivazione?: string | null;
  dataScadenzaPassword?: string | null;
  ultimoLogin?: string | null;
  dataDisattivazioneDate?: Date | null;
  dataRiattivazioneDate?: Date | null;
  dataScadenzaPasswordDate?: Date | null;
  ultimoLoginDate?: Date | null;
  dataInserimento: string;
  dataModifica?: string | null;
  stato?: string;
}

export interface RicercaUtenteRequest {
  codice?: string;
  cognome?: string;
  nome?: string;
  soloAttivi?: boolean;
}

export interface AggiornaUtenteRequest {
  codice: string;
  cognome: string;
  nome: string;
  codiceFiscale?: string | null;
  email?: string | null;
  matricola?: string | null;
  flagSmartCard: boolean;
  flagCambiaPwd: boolean;
  dataDisattivazione?: string | null;
}

export interface CreaUtenteRequest extends AggiornaUtenteRequest {}
