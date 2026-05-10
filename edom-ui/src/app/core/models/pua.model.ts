export interface Pua {
  id: number;
  numeroPuaId: number;
  numero: number;
  data: string;
  areaInterventoId: number;
  pazienteId: number;
  pazienteCognome: string;
  pazienteNome: string;
  pazienteCodiceFiscale?: string | null;
  pazienteDataNascita?: string | null;
  accessoId: number;
  accessoNote?: string | null;
  motivoId?: number | null;
  motivoNote?: string | null;
  richiestaId: number;
  richiestaAltro?: string | null;
  esitoId: number;
  esitoNote?: string | null;
  urgente: boolean;
  origineId: number;
  dataAvvio: string;
  dataChiusura?: string | null;
  motivoChiusuraId?: number | null;
  attivo: boolean;
}

export interface NumeroPuaDto {
  id: number;
  codice: string;
  anno: number;
  codiceAnno: string;
}

export interface AreaDto {
  id: number;
  codice: string;
  descrizione: string;
}

export interface PuaSearchRequest {
  pazienteId?: number;
  numeroPuaId?: number;
  attivo?: boolean;
  dataDa?: string;
  dataA?: string;
  take?: number;
}

export interface CreatePuaRequest {
  numeroPuaId: number;
  data: string;
  areaInterventoId: number;
  pazienteId: number;
  pazienteCognome: string;
  pazienteNome: string;
  pazienteCodiceFiscale?: string | null;
  accessoId: number;
  accessoNote?: string | null;
  motivoId?: number | null;
  motivoNote?: string | null;
  richiestaId: number;
  richiestaAltro?: string | null;
  esitoId: number;
  esitoNote?: string | null;
  urgente: boolean;
  origineId: number;
  dataAvvio: string;
  dataChiusura?: string | null;
  motivoChiusuraId?: number | null;
  attivo: boolean;
}

export interface UpdatePuaRequest extends CreatePuaRequest {}

export interface DuplicatePuaRequest {
  data?: string | null;
}
