# Dettaglio Tecnico Autenticazione con Refresh Token

## Obiettivo

L'autenticazione e' stata estesa da un modello basato sul solo access token JWT a un modello con:

1. Access token JWT a vita molto breve.
2. Refresh token persistito lato server.
3. Rotazione del refresh token a ogni refresh.
4. Revoca totale delle sessioni al logout.
5. Invalidazione immediata anche degli access token gia' emessi.

I vincoli implementati sono:

1. Access token con durata massima di 1 minuto.
2. Refresh token con durata massima di 8 ore.
3. Logout o chiusura della sessione logica con obbligo di nuova autenticazione.

## Architettura

Il flusso e' composto da due livelli:

1. Backend ASP.NET Core con JWT bearer authentication e storage server-side dei refresh token.
2. Frontend Angular con interceptor HTTP che esegue il refresh automatico in caso di 401.

### Principi adottati

1. Il refresh token non viene salvato in chiaro nel database, ma solo come hash SHA-256.
2. L'access token contiene claim `iat` e `jti` oltre ai claim utente standard.
3. Il backend mantiene uno stato per utente (`UserTokenState`) per invalidare immediatamente tutti gli access token emessi prima di un certo istante.
4. Il logout revoca tutte le sessioni refresh dell'utente e aggiorna `InvalidBeforeUtc`, forzando il rigetto immediato dei JWT gia' emessi.

## Componenti backend introdotti

### Entity: RefreshTokenSession

File:

1. [src/eDom.Core/Entities/RefreshTokenSession.cs](/home/alessio/repo_linux/edom_new/src/eDom.Core/Entities/RefreshTokenSession.cs)

Rappresenta un refresh token persistito lato server.

Campi principali:

1. `UserId`: utente proprietario.
2. `TokenHash`: hash SHA-256 del refresh token.
3. `FamilyId`: identificativo della famiglia di rotazione.
4. `CreatedAtUtc`: data emissione.
5. `ExpiresAtUtc`: scadenza massima del refresh token.
6. `RevokedAtUtc`: data revoca.
7. `ReplacedByTokenId`: token figlio generato dalla rotazione.
8. `CreatedByIp`, `RevokedByIp`: tracciamento IP.
9. `RevokedReason`: motivazione della revoca.

### Entity: UserTokenState

File:

1. [src/eDom.Core/Entities/UserTokenState.cs](/home/alessio/repo_linux/edom_new/src/eDom.Core/Entities/UserTokenState.cs)

Mantiene lo stato globale di invalidazione token di un utente.

Campi principali:

1. `UserId`
2. `InvalidBeforeUtc`
3. `UpdatedAtUtc`

Se un JWT contiene `iat <= InvalidBeforeUtc`, il token viene rifiutato anche se formalmente non ancora scaduto.

## Mapping EF Core

File:

1. [src/eDom.Infrastructure/Data/HctDbContext.cs](/home/alessio/repo_linux/edom_new/src/eDom.Infrastructure/Data/HctDbContext.cs)

Sono stati aggiunti:

1. `DbSet<RefreshTokenSession>`
2. `DbSet<UserTokenState>`

E le mappature verso le tabelle:

1. `APP_REFRESH_TOKENS`
2. `APP_USER_TOKEN_STATE`

Indici definiti:

1. Unico su `RFTK_TOKEN_HASH`
2. Indici su `RFTK_UTEN_ID`, `RFTK_FAMILY_ID`, `RFTK_EXPIRES_UTC`

## Migrazione database

File generati:

1. [src/eDom.Infrastructure/Data/Migrations/20260429165208_AddAuthRefreshTokenSupport.cs](/home/alessio/repo_linux/edom_new/src/eDom.Infrastructure/Data/Migrations/20260429165208_AddAuthRefreshTokenSupport.cs)
2. [src/eDom.Infrastructure/Data/Migrations/20260429165208_AddAuthRefreshTokenSupport.Designer.cs](/home/alessio/repo_linux/edom_new/src/eDom.Infrastructure/Data/Migrations/20260429165208_AddAuthRefreshTokenSupport.Designer.cs)

La migrazione crea le due nuove tabelle applicative per supportare refresh token e invalidazione globale access token.

## Factory token

File:

1. [src/eDom.Application/Features/Auth/AuthTokenFactory.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/AuthTokenFactory.cs)

Responsabilita:

1. Generazione access token JWT.
2. Generazione refresh token randomico.
3. Hash SHA-256 del refresh token.

### Access token

Viene costruito con:

1. claim `uid`
2. claim standard `sub`, `unique_name`, `given_name`
3. claim `jti`
4. claim `iat`
5. ruoli come `ClaimTypes.Role`

La scadenza e' hard-coded a 1 minuto per rispettare il vincolo di sicurezza.

### Refresh token

Il refresh token e' una stringa random base64url-safe generata con `RandomNumberGenerator.Fill` su 64 byte.

Il valore persistito lato server e' solo l'hash esadecimale SHA-256.

## Login flow

File:

1. [src/eDom.Application/Features/Auth/Login/LoginHandler.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/Login/LoginHandler.cs)
2. [src/eDom.Application/Features/Auth/Login/LoginCommand.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/Login/LoginCommand.cs)

Passi principali:

1. Validazione credenziali utente.
2. Aggiornamento `UltimoLogin`.
3. Scrittura record su `SI_LOGACC`.
4. Emissione access token da 1 minuto.
5. Emissione refresh token da 8 ore.
6. Persistenza di `RefreshTokenSession` con hash del token.

### Response di login

La response e' stata estesa con:

1. `token`
2. `expiresIn`
3. `refreshToken`
4. `refreshExpiresIn`
5. `username`
6. `fullName`
7. `roles`
8. `mustChangePassword`

## Refresh flow

File:

1. [src/eDom.Application/Features/Auth/RefreshToken/RefreshTokenCommand.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/RefreshToken/RefreshTokenCommand.cs)
2. [src/eDom.Application/Features/Auth/RefreshToken/RefreshTokenHandler.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/RefreshToken/RefreshTokenHandler.cs)

Logica implementata:

1. Ricezione del refresh token dal client.
2. Hash del token ricevuto.
3. Ricerca della sessione persistita.
4. Verifica di validita': non revocato, non scaduto.
5. Generazione nuovo access token.
6. Generazione nuovo refresh token.
7. Persistenza del nuovo refresh token nella stessa `FamilyId`.
8. Revoca del token precedente con motivo `rotated`.

### Reuse detection

E' stato introdotto un controllo base di riuso:

1. Se arriva un refresh token gia' revocato e con `ReplacedByTokenId` valorizzato, viene considerato riuso di un token ruotato.
2. In questo caso viene revocata l'intera famiglia (`FamilyId`).

Questo riduce il rischio di utilizzo di refresh token sottratti dopo una rotazione gia' avvenuta.

## Logout e revoca totale

File:

1. [src/eDom.Application/Features/Auth/Logout/LogoutCommand.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/Logout/LogoutCommand.cs)
2. [src/eDom.Application/Features/Auth/Logout/LogoutHandler.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/Logout/LogoutHandler.cs)

Comportamento:

1. Recupera l'utente corrente tramite `ICurrentUser`.
2. Revoca tutti i refresh token attivi dell'utente.
3. Aggiorna o crea `UserTokenState` impostando `InvalidBeforeUtc = now`.

Effetto:

1. Nessun refresh token preesistente puo' piu' essere usato.
2. Tutti gli access token emessi prima di `InvalidBeforeUtc` vengono rigettati immediatamente.

## Endpoint API

File:

1. [src/eDom.Api/Controllers/AuthController.cs](/home/alessio/repo_linux/edom_new/src/eDom.Api/Controllers/AuthController.cs)

Endpoint disponibili:

1. `POST /api/auth/login`
2. `POST /api/auth/refresh`
3. `GET /api/auth/permessi`
4. `POST /api/auth/change-password`
5. `POST /api/auth/logout`
6. `POST /api/auth/logout-all`

Nota: `logout` e `logout-all` al momento condividono la stessa implementazione, cioe' revoca totale delle sessioni dell'utente.

## Validazione JWT e invalidazione immediata

File:

1. [src/eDom.Api/Program.cs](/home/alessio/repo_linux/edom_new/src/eDom.Api/Program.cs)

Oltre alla validazione standard del JWT bearer, e' stato aggiunto un hook `OnTokenValidated` che:

1. legge il claim `uid`
2. legge il claim `iat`
3. carica `UserTokenState` dal database
4. fallisce la validazione se `iat <= InvalidBeforeUtc`

Questo realizza la revoca immediata degli access token senza attendere la naturale scadenza del minuto.

## Configurazione JWT

File:

1. [src/eDom.Api/appsettings.json](/home/alessio/repo_linux/edom_new/src/eDom.Api/appsettings.json)

Configurazione attuale:

1. `Issuer = eDom`
2. `Audience = eDomClient`
3. `ExpiryMinutes = 1`
4. `RefreshExpiryHours = 8`

Nota importante: il codice del token factory usa comunque access token da 1 minuto e refresh token da 8 ore come vincolo applicativo, indipendentemente da override parziali della configurazione.

## Frontend Angular

### Modelli auth

File:

1. [edom-ui/src/app/core/models/auth.models.ts](/home/alessio/repo_linux/edom_new/edom-ui/src/app/core/models/auth.models.ts)

Aggiornamenti:

1. `LoginResponse` include `refreshToken` e `refreshExpiresIn`.
2. Aggiunto `RefreshTokenRequest`.

### AuthService

File:

1. [edom-ui/src/app/core/services/auth.service.ts](/home/alessio/repo_linux/edom_new/edom-ui/src/app/core/services/auth.service.ts)

Funzioni principali:

1. `login()` salva access token e refresh token.
2. `refreshSession()` invoca `POST /auth/refresh`.
3. `logout()` invoca `POST /auth/logout` e pulisce lo storage locale.
4. `isAuthenticated` verifica la scadenza `exp` del JWT.

Storage attuale:

1. `access_token` in `localStorage`
2. `refresh_token` in `localStorage`

Nota: questa scelta e' coerente con l'implementazione attuale, ma meno sicura di cookie `HttpOnly` per il refresh token.

### Interceptor HTTP

File:

1. [edom-ui/src/app/core/interceptors/auth.interceptor.ts](/home/alessio/repo_linux/edom_new/edom-ui/src/app/core/interceptors/auth.interceptor.ts)

Comportamento:

1. Aggiunge header `Authorization: Bearer <token>` alle richieste.
2. Esclude login, refresh e logout dalla logica di retry.
3. Su `401` tenta un refresh automatico se e' disponibile un refresh token.
4. Usa un meccanismo single-flight con `refreshInFlight$` per evitare refresh concorrenti multipli.
5. Se il refresh fallisce, esegue logout completo del client.

## Sequenza operativa

### Login

1. Il client chiama `POST /api/auth/login`.
2. Il server valida le credenziali.
3. Il server restituisce access token e refresh token.
4. Il client salva entrambi nello storage locale.

### Accesso API standard

1. Il client invia l'access token nell'header Authorization.
2. Il backend valida firma, issuer, audience, lifetime e `InvalidBeforeUtc`.

### Refresh automatico

1. Se l'access token e' scaduto e una chiamata riceve `401`, l'interceptor chiama `POST /api/auth/refresh`.
2. Il server ruota il refresh token.
3. Il client salva la nuova coppia di token.
4. La richiesta originale viene ripetuta.

### Logout

1. Il client invia `POST /api/auth/logout`.
2. Il server revoca tutte le sessioni refresh dell'utente.
3. Il server aggiorna `InvalidBeforeUtc`.
4. Il client rimuove i token locali.
5. Ogni access token emesso in precedenza viene immediatamente rifiutato.

## Aspetti di sicurezza gia' coperti

1. Access token molto breve.
2. Refresh token persistito e hashed.
3. Rotazione refresh token.
4. Reuse detection base.
5. Revoca immediata access token via `InvalidBeforeUtc`.
6. Revoca totale lato utente al logout.

## Limiti attuali e possibili miglioramenti

### Limiti attuali

1. Il refresh token e' salvato in `localStorage`, non in cookie `HttpOnly`.
2. `logout` e `logout-all` hanno lo stesso comportamento.
3. Non e' presente un tracciamento device/session esplicito.
4. Non e' presente rate limiting dedicato su login/refresh.
5. La chiusura fisica del browser/tab non puo' garantire da sola la chiamata di logout server-side in modo affidabile.

### Miglioramenti consigliati

1. Spostare il refresh token in cookie `HttpOnly` + `Secure` + `SameSite` adeguato.
2. Distinguere sessione corrente da logout globale tramite `SessionId` o `DeviceId`.
3. Aggiungere rate limiting specifico sugli endpoint auth.
4. Aggiungere audit event espliciti per refresh, revoca e reuse detection.
5. Implementare cleanup periodico dei refresh token scaduti/revocati.

## Verifiche eseguite

Sono state eseguite:

1. build backend della soluzione
2. build frontend Angular
3. generazione migrazione EF Core

Al momento della scrittura del documento, la migrazione e' stata generata ma deve essere applicata al database per rendere il sistema pienamente operativo.

## File principali coinvolti

Backend:

1. [src/eDom.Api/Controllers/AuthController.cs](/home/alessio/repo_linux/edom_new/src/eDom.Api/Controllers/AuthController.cs)
2. [src/eDom.Api/Program.cs](/home/alessio/repo_linux/edom_new/src/eDom.Api/Program.cs)
3. [src/eDom.Application/Features/Auth/AuthTokenFactory.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/AuthTokenFactory.cs)
4. [src/eDom.Application/Features/Auth/Login/LoginHandler.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/Login/LoginHandler.cs)
5. [src/eDom.Application/Features/Auth/RefreshToken/RefreshTokenHandler.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/RefreshToken/RefreshTokenHandler.cs)
6. [src/eDom.Application/Features/Auth/Logout/LogoutHandler.cs](/home/alessio/repo_linux/edom_new/src/eDom.Application/Features/Auth/Logout/LogoutHandler.cs)
7. [src/eDom.Infrastructure/Data/HctDbContext.cs](/home/alessio/repo_linux/edom_new/src/eDom.Infrastructure/Data/HctDbContext.cs)
8. [src/eDom.Infrastructure/Data/Migrations/20260429165208_AddAuthRefreshTokenSupport.cs](/home/alessio/repo_linux/edom_new/src/eDom.Infrastructure/Data/Migrations/20260429165208_AddAuthRefreshTokenSupport.cs)

Frontend:

1. [edom-ui/src/app/core/models/auth.models.ts](/home/alessio/repo_linux/edom_new/edom-ui/src/app/core/models/auth.models.ts)
2. [edom-ui/src/app/core/services/auth.service.ts](/home/alessio/repo_linux/edom_new/edom-ui/src/app/core/services/auth.service.ts)
3. [edom-ui/src/app/core/interceptors/auth.interceptor.ts](/home/alessio/repo_linux/edom_new/edom-ui/src/app/core/interceptors/auth.interceptor.ts)
