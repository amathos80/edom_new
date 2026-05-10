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
  telefono2?: string;
  indirizzoResidenza?: string;
  capResidenza?: string;
  indirizzoDomicilio?: string;
  capDomicilio?: string;
  medicoId?: number;
  attivo: boolean;
  dataInserimento: string;
}

export interface PazientePuaData {
  id: number;
  codice: string;
  cognome: string;
  nome: string;
  nomeCompleto: string;
  dataNascita: string;
  codiceFiscale?: string;
  sesso?: string;
  email?: string;
  telefono1?: string;
  telefono2?: string;
  comuneResidenzaDescr?: string;
  indirizzoResidenza?: string;
  capResidenza?: string;
  comuneDomicilioDescr?: string;
  indirizzoDomicilio?: string;
  capDomicilio?: string;
  comuneReperibilitaDescr?: string;
  indirizzoReperibilita?: string;
  capReperibilita?: string;
  nomeCampanelloReperibilita?: string;
  areaResidenzaId: number;
  areaDomicilioId?: number;
  areaReperibilitaId?: number;
  medicoCodice?: string;
  medicoNominativo?: string;
  medicoEmail?: string;
  medicoTelefono1?: string;
  medicoTelefono2?: string;
  
}

export interface PazienteSearchRequest {
  cognome?: string;
  nome?: string;
  codiceFiscale?: string;
  dataNascita?: string;
  attivo?: boolean;
  page?: number;
  pageSize?: number;
}

