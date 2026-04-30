# Auth Permissions Client Cache

## Obiettivo
- Mantenere il JWT piccolo (identita + ruoli essenziali).
- Ottenere le funzioni utente da endpoint dedicato.
- Persistenza client delle funzioni per non perderle al refresh.
- Evitare una chiamata permessi su ogni API call applicativa.

## Strategia
- Endpoint backend dedicato: `GET /api/auth/permissions` (autenticato).
- Payload restituito:
  - `username`
  - `fullName`
  - `roles`
  - `functions` (union distinta delle funzioni abilitate tramite ruoli)
- Frontend:
  - token in `localStorage` (gia esistente)
  - contesto autorizzativo in `localStorage` (`auth_permissions_v1`)
  - `AuthService.assicuraPermessiCaricati()`:
    - se non autenticato: ritorna `null`
    - se cache presente: usa cache
    - altrimenti: chiama `/api/auth/permissions`, salva cache, espone signal
- `MenuService` filtra il menu usando `functions` del contesto autorizzativo.

## Flusso
1. Login riuscito:
   - salva token
   - decodifica token
   - carica `/api/auth/permissions`
   - persiste `auth_permissions_v1`
2. Refresh pagina:
   - token riletto da storage
   - contesto permessi riletto da storage
   - menu renderizzato con le funzioni cached
3. Logout:
   - rimozione token e cache permessi

## Trade-off
- Pro:
  - JWT contenuto
  - menu corretto anche dopo refresh
  - niente chiamata permessi per ogni API
- Contro:
  - possibile lieve staleness fino al prossimo refresh dei permessi

## Invalidazione consigliata
- Invalida la cache permessi su:
  - login/logout
  - 401/403 globali (opzionale)
  - endpoint amministrativo che modifica autorizzazioni (se disponibile)

## Stato implementazione
- Backend endpoint `GET /api/auth/permissions`: implementato.
- Frontend cache permessi + bootstrap lazy via `assicuraPermessiCaricati()`: implementato.
- Menu filtrato su `functions` anziche su `roles` dal JWT: implementato.
