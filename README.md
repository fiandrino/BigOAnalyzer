# BigOAnalyzer — Analisi empirica della complessità (Big O)

Piccolo progetto C# che stima empiricamente la complessità computazionale (Big O)
di un algoritmo misurandone i tempi di esecuzione su input di dimensione crescente,
e confrontando la crescita osservata con i modelli di complessità più comuni tramite
regressione (R²).

## Struttura del progetto

```
BigOAnalyzer/
├── Algorithms.cs       # algoritmi di esempio (insertionSort, mergeSort, LinSrc, BinSrc, HasTripletSum)
├── BigOAnalyzer.cs     # il motore di analisi Big O
├── Program.cs          # esempi d'uso su tutti gli algoritmi
└── BigOAnalyzer.csproj
```

## Esecuzione

```
cd BigOAnalyzer
dotnet run
```

L'output riporta, per ogni algoritmo analizzato, la complessità stimata:

```
insertionSort: O(n^2)
mergeSort: O(n log n)
HasTripletSum: O(n^3)
LinSrc: O(n)
BinSrc: O(log n)
```

## Guida all'uso di `BigOAnalyzer`

`BigOAnalyzer.Analyze` prende un algoritmo, lo esegue su input di dimensione crescente,
misura i tempi e li confronta con 6 modelli di complessità candidati
(`O(1)`, `O(log n)`, `O(n)`, `O(n log n)`, `O(n^2)`, `O(n^3)`), scegliendo quello con il
miglior adattamento (R² più alto).

### Firma

```csharp
BigOAnalyzer.Analyze(
    string label,
    Func<int, int[]> arrayFactory,
    Action<int[]> run,
    int[] sizes,
    int repetitions = 5,
    bool mutatesInput = true,
    int trials = 5);
```

| Parametro       | Significato |
|-----------------|-------------|
| `label`         | Nome dell'algoritmo, mostrato nel report. |
| `arrayFactory`  | Genera un array di input di dimensione `n`, nel **caso peggiore** per l'algoritmo. |
| `run`           | Esegue l'algoritmo sull'array. Se l'algoritmo restituisce un valore (es. una ricerca), basta scrivere `a => Algoritmo(a, ...)`: il valore di ritorno viene scartato automaticamente. |
| `sizes`         | Le dimensioni `n` da misurare, in ordine crescente. |
| `repetitions`   | Quante volte ripetere la misura *all'interno* di un singolo trial. |
| `mutatesInput`  | `true` se `run` modifica l'array (es. un sort): serve un array fresco a ogni ripetizione. `false` se `run` si limita a leggerlo (es. una ricerca): lo stesso array viene riusato per tutte le ripetizioni, per non sprecare memoria. |
| `trials`        | Quante volte ripetere l'intera batch di misure per ogni `n`. Si usa la **mediana** dei trial (non la media), per non far falsare la stima da un singolo trial disturbato da una pausa del garbage collector o dal sistema operativo. |

### Esempio: un algoritmo che modifica l'array (sort)

```csharp
BigOAnalyzer.Analyze(
    "insertionSort",
    size =>
    {
        // caso peggiore: array in ordine decrescente
        int[] a = new int[size];
        for (int i = 0; i < size; i++) a[i] = size - i;
        return a;
    },
    Algorithms.insertionSort,
    sizes: [1_000, 2_000, 4_000, 8_000, 16_000],
    repetitions: 1,
    mutatesInput: true,
    trials: 5);
```

### Esempio: un algoritmo che legge soltanto l'array (ricerca), con argomenti extra

```csharp
BigOAnalyzer.Analyze(
    "LinSrc",
    BuildSortedArray, // genera un array ordinato di dimensione n
    a => Algorithms.LinSrc(a, -1), // -1 = caso peggiore: elemento non presente
    sizes: [200_000, 400_000, 800_000, 1_600_000, 3_200_000],
    repetitions: 30,
    mutatesInput: false,
    trials: 5);
```

## Cosa serve per testare un nuovo algoritmo

`BigOAnalyzer` funziona con **qualsiasi algoritmo che riceve un `int[]`** come input
(mutante o meno). Per aggiungerne uno:

1. Implementalo in `Algorithms.cs`.
2. Scrivi un `arrayFactory` che generi il **caso peggiore** per quell'algoritmo alla
   dimensione `n` (es. array decrescente per un sort, elemento assente per una ricerca).
3. Se l'algoritmo richiede argomenti oltre all'array, avvolgilo in una lambda:
   `a => Algoritmo(a, altroArgomento)`.
4. Imposta `mutatesInput` correttamente (`true` se l'algoritmo altera l'array).
5. Scegli un range di `sizes` **abbastanza ampio**: su un intervallo troppo stretto
   modelli vicini come `O(n)` e `O(n log n)` producono curve quasi indistinguibili.
   Un fattore di almeno ~50-100× tra il valore minimo e massimo di `n` dà risultati
   più affidabili.

### Limiti noti

- Funziona solo su `int[]`: per testare algoritmi su altri tipi (stringhe, oggetti,
  strutture generiche) la classe andrebbe resa generica (`T[]`).
- La stima è **euristica**, basata su misurazioni reali soggette a rumore di sistema
  (per questo esistono `trials` e la mediana): non sostituisce un'analisi teorica.
- Per algoritmi estremamente veloci (poche decine di nanosecondi per chiamata, es.
  ricerca binaria su array piccoli) le misure sono vicine alla risoluzione del timer:
  aumentare `repetitions` e/o `trials` aiuta a stabilizzare la stima.
