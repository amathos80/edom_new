export interface Procedura {
  id: number;
  codice: string;
  descrizione: string;
  visibile: boolean;
}

export interface RicercaProceduraRequest {
  codice?: string;
  descrizione?: string;
  soloVisibili?: boolean;
}
