export interface Paziente {
  id: number;
  codice: string;
  cognome: string;
  nome: string;
  nomeCompleto: string;
  dataNascita: string;
  codiceFiscale: string;
  sesso: string;
  email?: string;
  codiceSanitario?: string;
  telefono1?: string;
  indirizzoResidenza?: string;
  capResidenza?: string;
  medicoId?: number;
  attivo: boolean;
  dataInserimento: string;
}

export interface PazienteSearchRequest {
  cognome?: string;
  codiceFiscale?: string;
  attivo?: boolean;
}
