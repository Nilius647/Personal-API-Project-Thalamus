# Thalamus — Checklist Fase 3

**Obiettivo:** trasformare dati grezzi in risposte. Alla fine della fase, un metodo restituisce "3 ore su VS Code oggi" invece di farti scorrere a mano centinaia di `Sample`.

**Punto d'arrivo concreto:** `thalamus stats` mostra numeri che confermano quello che sai essere vero della tua giornata.

**Il pezzo centrale:** `DataManager`, la facade che il resto dell'app chiamerà sempre. Nessuno tocca `IRepository` direttamente fuori da qui.

---

## Nota sul metodo

Come nella Fase 2: questa checklist dice cosa creare e cosa deve fare, non come. Il codice lo scrivi tu.

Il punto D — la logica di aggregazione — è dove incontrerai la prima vera complessità del progetto. `GroupBy` in LINQ è diverso da `Where` e `Select` che conosci già. Prenditi tempo lì.

---

## PARTE A — Preparazione

### A1. Dati reali

- [ ] `collect` deve girare da più giorni possibile prima di scrivere aggregazioni
- [ ] Non progettare le query guardando quattro campioni di test — guarda il tuo `.db` reale con DB Browser

> Un'aggregazione scritta contro dati finti spesso nasconde casi che i dati veri mostrano subito: buchi di campionamento, sessioni che attraversano la mezzanotte, boot senza shutdown.

### A2. Formalizza le domande

Per ognuna delle tre domande originali, scrivi la formula esatta:

- [ ] Domanda 1 — "ore accese senza uso" = ?
- [ ] Domanda 2 — "tempo per app in un intervallo" = ?
- [ ] Domanda 3 — "orari di accensione/spegnimento" = ?

> Se non riesci a scrivere la formula in una riga, la query che dovrai fare non è ancora chiara. Risolvilo qui, non nel codice.

---

## PARTE B — Tipi di supporto

### B1. Cartella

- [ ] `src/Thalamus.Core/Aggregation/`

### B2. `Summary.cs`

Un record che rappresenta un risultato aggregato. Contiene almeno:

- [ ] Il `DateRange` a cui si riferisce
- [ ] Tempo totale attivo
- [ ] Tempo totale idle
- [ ] Un breakdown per app (nome processo → durata)

### B3. `DailyUsage.cs` (se serve)

Un giorno con le sue statistiche — utile per il confronto settimana-su-settimana della domanda 2.

- [ ] Data
- [ ] Tempo attivo totale
- [ ] Eventualmente il breakdown per app di quel giorno

> Valuta se ti serve davvero un tipo separato o se `Summary` con un `DateRange` di un solo giorno basta. Non introdurre un tipo se non risolve un problema concreto.

---

## PARTE C — DataManager

### C1. File

- [ ] `src/Thalamus.Core/DataManager.cs`

### C2. Struttura

- [ ] Riceve `IRepository` nel costruttore — stessa dependency injection già usata ovunque
- [ ] Nessun riferimento a SQLite, Dapper, o dettagli di persistenza: `DataManager` parla solo con `IRepository`

### C3. I tre metodi

- [ ] Metodo per la domanda 1: tempo totale nel periodo vs tempo effettivamente attivo, più stima di consumo
- [ ] Metodo per la domanda 2: riepilogo per app in un intervallo, con confronto verso il periodo precedente di pari durata
- [ ] Metodo per la domanda 3: elenco delle sessioni (accensione → spegnimento) negli ultimi N giorni

> Ogni metodo dovrebbe poter essere descritto in una frase a chi non conosce il codice. Se la descrizione richiede una subordinata, probabilmente il metodo fa troppo.

---

## PARTE D — La logica di aggregazione

### D1. Cosa "vale" un campione

- [ ] Decidi e documenta: se campioni ogni 5 secondi, ogni `Sample` rappresenta 5 secondi di tempo?
- [ ] Cosa succede se il collector si è fermato per un periodo (PC spento, app crashata)? Un buco tra due campioni non deve essere contato come tempo attivo

> Questa è l'assunzione su cui poggia ogni conteggio di ore. Sbagliarla qui significa numeri sbagliati ovunque, silenziosamente.

### D2. Raggruppamento per giorno

- [ ] Da una lista di `Sample` con timestamp sparsi, ottieni bucket per data — `GroupBy` su `TimestampUtc.Date`
- [ ] Attenzione al fuso: un campione a UTC 23:30 e uno a UTC 00:30 del giorno dopo sono lo stesso "giorno" per l'utente, se la conversione a ora locale li mette nello stesso pomeriggio/sera?

### D3. Somma per app

- [ ] Raggruppa i campioni non-idle per `ProcessName`
- [ ] Somma le durate secondo la regola di D1
- [ ] Ordina per durata decrescente — è quello che vuoi vedere per primo

### D4. Boot orfani

Hai già osservato il caso reale: un `Boot` senza `Shutdown` corrispondente prima del prossimo `Boot`.

- [ ] Decidi la regola: la sessione dura fino al prossimo `Boot`? Fino all'ultimo `Sample` registrato? Viene marcata come "durata sconosciuta" e esclusa dal calcolo?
- [ ] Scrivi la regola scelta come commento nel codice, non solo a parole

> Qualunque regola scegli, sarà sbagliata in qualche caso limite. L'importante è che sia esplicita e testata, non implicita.

### D5. Stima del consumo

- [ ] Da dove viene il wattaggio? Costante nel `Profile`, o valore fisso nel codice per ora?
- [ ] Formula: ore accese × wattaggio stimato = stima consumo

> Rimandare questo è legittimo se il wattaggio nel `Profile` non è ancora stato aggiunto. Documenta che è una stima grezza, non una misurazione.

---

## PARTE E — Test

### E1. Dataset di test controllato

- [ ] Costruisci campioni ed eventi con orari scelti a mano, non casuali
- [ ] Calcola tu stesso, con carta e penna, il risultato atteso prima di scrivere il test

> Se non sai qual è la risposta giusta prima di far girare il codice, il test non verifica niente — conferma solo che il codice fa quello che fa.

### E2. Casi da coprire

- [ ] Un giorno con campioni continui, nessuna interruzione — il caso semplice
- [ ] Un `Boot` senza `Shutdown` corrispondente — verifica la regola di D4
- [ ] Un buco nel campionamento (es. PC spento per ore) — verifica che non venga contato come tempo attivo
- [ ] Confronto tra due periodi consecutivi (settimana corrente vs precedente)
- [ ] Un intervallo senza dati — deve restituire un `Summary` vuoto, non lanciare eccezioni

---

## PARTE F — Collegamento alla CLI

### F1. Comando temporaneo

- [ ] Aggiungi un ramo `stats` allo switch in `Program.cs`
- [ ] Non è ancora la Fase 4 — resta un test manuale, non il sistema di comandi vero

### F2. Verifica a vista

- [ ] Lancia `stats` e confronta con quello che sai essere vero della tua giornata
- [ ] Se i numeri non tornano, il problema è quasi sempre in D1 o D2 — l'assunzione su cosa vale un campione, o il raggruppamento per giorno

---

## PARTE G — Chiusura

### G1. Documentazione

- [ ] Aggiorna `ARCHITECTURE.md`: la regola sui boot orfani, l'unità di aggregazione (durata per campione), da dove viene il wattaggio
- [ ] Aggiorna il README: la Fase 3 è chiusa, magari con un esempio di output di `stats`

### G2. Verifica

- [ ] `dotnet build` senza warning
- [ ] `dotnet test` verde
- [ ] La CI passa

### G3. Versione e Git

- [ ] `AppInfo.Version` → `0.3.0`, aggiorna il test corrispondente
- [ ] Commit e push
- [ ] Tag `v0.3.0-aggregation`

---

## Errori tipici

| Problema | Causa probabile |
|---|---|
| Le ore totali non tornano | Assunzione sbagliata su cosa "vale" un campione (D1) |
| Un giorno mostra ore doppie o dimezzate | Raggruppamento per data che non tiene conto del fuso orario (D2) |
| Il tempo acceso include periodi di PC spento | Buco nel campionamento non gestito (D1) |
| I confronti tra periodi danno numeri assurdi | I due intervalli hanno durate diverse — confronta sempre periodi di pari lunghezza |
| `stats` esplode su un intervallo vuoto | Manca la gestione del caso "nessun dato" nel metodo di `DataManager` |

---

## Prossimo passo

Chiusa la Fase 3, hai risposte vere alle tue tre domande — ma solo da riga di comando, in forma grezza.

La Fase 4 costruisce il vero sistema di comandi: `ICommand`, un registro, i comandi di sistema dal tuo progetto C++. È anche il momento in cui lo `switch` temporaneo di `Program.cs` viene finalmente smontato per bene.