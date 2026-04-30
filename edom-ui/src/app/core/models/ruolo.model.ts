export interface Ruolo {
  id: number;
  proceduraId: number;
  proceduraCodice?: string | null;
  codice: string;
  descrizione: string;
  flagAmministratore: boolean;
  dataInserimento: string;
  dataModifica?: string | null;
}

export interface RicercaRuoloRequest {
  codice?: string;
  descrizione?: string;
  flagAmministratore?: boolean;
  proceduraId?: number;
}

export interface CreaRuoloRequest {
  proceduraId: number;
  codice: string;
  descrizione: string;
  flagAmministratore: boolean;
}

export interface AggiornaRuoloRequest extends CreaRuoloRequest {}

export interface RuoloFunzioneNodo {
  id: number;
  codice: string;
  descrizione: string;
  parentId?: number | null;
  selezionata: boolean;
  figlie: RuoloFunzioneNodo[];
}
