# Thalamus — Checklist Fase 2

**Obiettivo:** dati veri nel database. Alla fine della fase, il tuo PC produce campioni ed eventi che puoi interrogare.

**Punto d'arrivo concreto:** lanci `thalamus collect`, lo lasci girare mezz'ora, poi `thalamus list` ti mostra cosa hai fatto.

**Le tre domande di riferimento:**
1. Quanto è stato acceso il PC senza che lo usassi (e stima consumo)
2. Quanto tempo al PC questa settimana vs le scorse, e con quali app
3. A che ora ho acceso/spento il PC negli ultimi X giorni

---

## Nota sul metodo

Questa checklist elenca **cosa** creare e **cosa deve fare**, non come. Il codice lo scrivi tu.

Quando ti blocchi, la sequenza utile è: leggi la documentazione Microsoft del tipo che ti serve, prova, e solo dopo cerca un esempio. Copiare soluzioni prima di aver capito il problema è il modo più veloce per non imparare niente.

Compila e testa dopo ogni parte. Non arrivare in fondo e poi debuggare tutto insieme.

---

## PARTE A — Modelli

### A1. Cartella `src/Thalamus.Core/Models/`

- [x] `Sample.cs` — un campione di stato del PC in un istante
- [x] `SystemEvent.cs` — un evento discreto (accensione, spegnimento, blocco…)
- [x] `EventKind.cs` — enum: Boot, Shutdown, Lock, Unlock, Sleep, Wake
- [x] `Profile.cs` — un profilo utente con nome e date

### A2. Requisiti dei modelli

Ogni record deve avere:

- [x] `Guid Id` — **non** un intero incrementale
- [x] `Guid ProfileId` — a quale profilo appartiene
- [x] `DateTime TimestampUtc` — sempre UTC, mai ora locale
- [x] `bool IsDeleted` — cancellazione logica

> I tre vincoli servono a rendere possibile la sincronizzazione futura. Costano una riga ciascuno adesso; aggiungerli dopo significa migrare tutto il database.

`Sample` deve inoltre catturare: nome del processo, titolo finestra, se il sistema è idle, e da quanti secondi.

`Profile` deve avere: nome scelto dall'utente, data creazione, e il wattaggio stimato del PC (serve per la domanda 1).

### A3. Decisioni da prendere

- [x] `record` o `class`? — considera che questi oggetti non dovrebbero cambiare dopo la creazione
- [x] Cosa succede se `ProcessName` è null perché il PC è idle? — decidi ora, documenta la scelta
- [x] Ogni quanto campionare? — più frequente significa più precisione e più righe

> Suggerimento sul terzo punto: 5 secondi produce circa 17.000 righe al giorno. SQLite lo regge senza problemi, ma fai il conto su un anno prima di scegliere.

### A4. Test

- [x] Un `Sample` creato ha un `Id` non vuoto
- [x] Il timestamp è in UTC, non locale

---

## PARTE B — Contratto di storage

### B1. Cartella `src/Thalamus.Core/Storage/`

- [x] `IRepository.cs` — l'interfaccia
- [x] `DateRange.cs` — un intervallo temporale con validazione
- [x] `MemoryRepository.cs` — implementazione in memoria

### B2. Cosa deve esporre `IRepository`

Progettala guardando le tre domande, non in astratto. Ti servono operazioni per:

- [x] Aggiungere campioni — in blocco, non uno alla volta
- [x] Aggiungere eventi di sistema
- [x] Recuperare campioni in un intervallo
- [x] Recuperare eventi in un intervallo, filtrabili per tipo
- [x] Gestire i profili (creare, elencare, selezionare l'attivo)

> Domanda da porti: i metodi devono essere `async`? Considera che in Fase 6 potrebbero esserci collector che chiamano API di rete.

### B3. `MemoryRepository`

Implementazione basata su liste in memoria. Serve ai test: istantanea, nessun file da pulire.

- [x] Rispetta l'interfaccia per intero
- [x] Ignora i record con `IsDeleted = true` nelle query

### B4. Test di contratto

- [x] `tests/Thalamus.Tests/Storage/RepositoryContractTests.cs`

Scrivi i test **contro `IRepository`**, non contro l'implementazione concreta. In xUnit questo si fa con una classe base astratta, o con `Theory` e `MemberData`.

Casi minimi:

- [x] Salvo N campioni, ne rileggo N
- [x] Query su intervallo restituisce solo ciò che è dentro
- [x] Record cancellati logicamente non compaiono
- [x] Query su intervallo vuoto restituisce lista vuota, non errore

> Questi stessi test dovranno passare su SQLite in Parte C, senza modifiche. Se ci riesci, l'astrazione regge davvero.

---

## PARTE C — SQLite

### C1. Pacchetti

- [x] Aggiungi `Dapper` a `Thalamus.Core`
- [x] Aggiungi `Microsoft.Data.Sqlite` a `Thalamus.Core`

### C2. File

- [x] `Storage/SqliteRepository.cs` — implementazione reale
- [x] `Storage/Schema.cs` — le istruzioni CREATE TABLE
- [x] `Storage/DbConnectionFactory.cs` — apre connessioni e applica lo schema

### C3. Schema del database

Scrivi tu l'SQL. Due tabelle principali (`samples`, `system_events`) più `profiles`.

Cose a cui pensare:

- [x] Che tipo usare per i GUID in SQLite? (SQLite non ha un tipo nativo)
- [x] Come salvare i DateTime perché restino ordinabili e confrontabili?
- [x] Quali indici servono? — guarda le tue tre domande: interroghi quasi sempre per intervallo temporale
- [x] Vincoli di chiave esterna tra samples e profiles

> L'indice sul timestamp non è un dettaglio: senza, la domanda 2 su un anno di dati diventa lentissima.

### C4. Versioning dello schema

- [ ] Una tabella che registra la versione dello schema

Non ti serve oggi. Ti servirà la prima volta che aggiungi una colonna a un database che contiene già i tuoi dati — e a quel punto averla è la differenza tra una migrazione e ricominciare da zero.

### C5. Test

- [x] Fai girare `RepositoryContractTests` anche su `SqliteRepository`
- [x] Usa un file temporaneo, o SQLite in-memory, mai il database reale

---

## PARTE D — Profili

### D1. File

- [x] `Storage/ProfileManager.cs` — crea, elenca, seleziona il profilo attivo
- [x] `Storage/AppPaths.cs` — dove vivono i file

### D2. Struttura su disco

```
%APPDATA%/Thalamus/
├── profiles.json          elenco e metadati
└── profiles/
    └── {guid}.db          un database per profilo
```

- [x] Un file per profilo, non una tabella condivisa
- [x] La cartella si crea al primo avvio se non esiste

> Un file per profilo significa che esportare, fare backup o cambiare PC è copiare un file. È già l'80% della sincronizzazione, senza scrivere un server.

Usa `Environment.GetFolderPath` per trovare `%APPDATA%`, non una stringa hardcoded.

### D3. Test

- [x] Creare un profilo produce un database utilizzabile
- [x] Due profili non vedono i dati l'uno dell'altro

---

## PARTE E — Il primo collector

### E1. Nuovo progetto

```powershell
dotnet new classlib -o src/Thalamus.Collectors
dotnet sln add src/Thalamus.Collectors/Thalamus.Collectors.csproj
dotnet add src/Thalamus.Collectors reference src/Thalamus.Core
```

- [x] Questo progetto **può** essere `net10.0-windows`. Il Core no.

### E2. File

- [x] `ICollector.cs` — nel Core, non qui: è un contratto
- [x] `Windows/NativeMethods.cs` — le dichiarazioni P/Invoke
- [x] `Windows/ActivityCollector.cs` — finestra attiva e stato idle
- [x] `Windows/SessionEventCollector.cs` — blocco, sblocco, sospensione

### E3. Le API di Windows che ti servono

Cercale nella documentazione Microsoft, la firma P/Invoke si deduce da lì:

- [x] `GetForegroundWindow` — handle della finestra in primo piano
- [x] `GetWindowThreadProcessId` — da handle a process ID
- [x] `GetLastInputInfo` — millisecondi dall'ultimo input di mouse o tastiera
- [x] `SystemEvents.SessionSwitch` — evento .NET, non P/Invoke: blocco e sblocco

Da process ID a nome del processo ci arrivi con `System.Diagnostics.Process`, senza P/Invoke.

> P/Invoke è il meccanismo per chiamare codice C da C#. Il concetto chiave è il marshalling: tradurre i tipi tra i due mondi. Se una struttura non è dichiarata con la dimensione giusta, la chiamata fallisce silenziosamente invece di dare errore — è l'insidia tipica.

### E4. Cosa deve fare `ActivityCollector`

- [x] Campiona a intervallo configurabile
- [x] Determina se il sistema è idle (soglia da decidere: 60 secondi? 300?)
- [x] Se attivo, registra processo e titolo finestra
- [x] Se idle, registra comunque il campione — **è quello che risponde alla domanda 1**
- [x] Non esplode se una finestra non ha titolo o il processo termina durante la lettura

> L'ultimo punto capiterà. Il processo può chiudersi tra `GetForegroundWindow` e la lettura del nome.

### E5. Accensioni e spegnimenti

Per la domanda 3 servono gli eventi di boot e shutdown. Sono nell'Event Log di Windows, canale System, ID 6005 (avvio) e 6006 (arresto).

- [x] `Windows/EventLogCollector.cs`
- [x] Usa `System.Diagnostics.Eventing.Reader`
- [x] Va bene leggerli all'avvio dell'app, non in continuo

> Verifica se serve eseguire come amministratore. Il canale System di solito è leggibile, il Security no.

### E6. Test

I collector toccano il sistema operativo, quindi sono difficili da testare in isolamento. Testa quello che puoi separare:

- [x] La logica di conversione da millisecondi idle a `IsIdle` (estraila in un metodo puro)
- [x] La gestione di titolo finestra null o vuoto
- [x] Non testare `GetForegroundWindow` — è codice di Microsoft

---

## PARTE F — Collegare tutto

### F1. Comandi CLI minimi

Non è ancora la Fase 4, servono solo per verificare che la catena funzioni:

- [x] `collect` — avvia il campionamento, si ferma con Ctrl+C
- [x] `list` — mostra gli ultimi N campioni
- [x] `profiles` — elenca i profili

### F2. Il ciclo di raccolta

- [x] Il collector produce campioni, il repository li salva
- [x] Salvataggio a lotti, non uno alla volta — una transazione per riga è lentissima
- [x] Chiusura pulita su Ctrl+C: usa `Console.CancelKeyPress` e svuota il buffer prima di uscire

### F3. Prova reale

- [x] Lancia `collect` e lascialo girare 30 minuti mentre lavori normalmente
- [x] `list` mostra le app che hai realmente usato
- [x] Apri il `.db` con [DB Browser for SQLite](https://sqlitebrowser.org) e guarda le righe

> Aprire il database a mano la prima volta è utile: vedi cosa hai davvero scritto, non cosa credi di aver scritto.

---

## PARTE G — Chiusura

### G1. Verifica

- [x] `dotnet build` senza warning
- [x] `dotnet test` verde
- [x] La CI su GitHub passa

> Attenzione: `Thalamus.Collectors` è Windows-only, quindi la build su `ubuntu-latest` fallirà. Due opzioni: passa il workflow a `windows-latest`, oppure escludi quel progetto dalla build CI. Decidi e documenta.

### G2. Documentazione

- [x] Aggiorna `ARCHITECTURE.md` con lo schema del database
- [x] Annota le decisioni prese: intervallo di campionamento, soglia idle, formato dei GUID
- [x] Aggiorna il README: la Fase 2 è chiusa

### G3. Git

- [x] **Verifica che `data/` e i file `.db` non siano nel commit**
- [x] Commit e push
- [x] Tag `v0.2.0-storage`

---

## Ordine consigliato

Non seguire le lettere alla cieca. Questo ordine ti dà qualcosa di verificabile prima:

1. **A** — Modelli, sono veloci
2. **B** — Interfaccia e MemoryRepository, con i test di contratto
3. **E4 parziale** — Solo `GetForegroundWindow` e `GetLastInputInfo`, stampando a console

> Fermati qui e guarda l'output. Vedere il nome della finestra attiva stampato in tempo reale è il momento in cui il progetto smette di essere astratto.

4. **C** — SQLite, ora che sai esattamente cosa salvare
5. **D** — Profili
6. **E5, F** — Event Log e comandi CLI

---

## Errori tipici

| Problema | Causa |
|---|---|
| I timestamp non tornano | Hai salvato ora locale invece di UTC da qualche parte |
| Query lentissime su molti dati | Manca l'indice sul timestamp |
| Il P/Invoke restituisce sempre zero | Struttura dichiarata con dimensione sbagliata |
| Il DB cresce a dismisura | Intervallo di campionamento troppo fitto |
| Il processo non si trova | È terminato tra la lettura dell'handle e quella del nome |
| Test lenti | Stanno usando SQLite su file invece che in memoria |

---

## Prossimo passo

Chiusa la Fase 2, hai dati grezzi ma non ancora risposte.

La Fase 3 costruisce `DataManager` e le aggregazioni: trasformare 17.000 campioni al giorno in "3 ore su Visual Studio, 40 minuti idle". È lì che le tue tre domande diventano metodi.

Quando ti blocchi su qualcosa di specifico, chiedi. Ma prova prima — il punto della fase è quello.