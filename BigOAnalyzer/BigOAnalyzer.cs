using System.Diagnostics;

namespace BigOAnalyzerApp;

/// <summary>
/// Stima empiricamente la complessità (Big O) di un algoritmo misurandone i tempi
/// di esecuzione su input di dimensione crescente e confrontando la crescita
/// osservata con alcuni modelli di complessità noti tramite regressione (R²).
/// </summary>
internal static class BigOAnalyzer
{
    private static readonly (string Name, Func<double, double> F)[] Models =
    {
        ("O(1)", _ => 1.0),
        ("O(log n)", n => Math.Log2(n)),
        ("O(n)", n => n),
        ("O(n log n)", n => n * Math.Log2(n)),
        ("O(n^2)", n => n * n),
        ("O(n^3)", n => n * n * n),
    };

    /// <param name="label">Nome dell'algoritmo, mostrato nel report.</param>
    /// <param name="arrayFactory">Genera un array di input di dimensione n (caso peggiore).</param>
    /// <param name="run">Esegue l'algoritmo sull'array fornito (il risultato viene ignorato).</param>
    /// <param name="sizes">Le dimensioni di input da misurare, in ordine crescente.</param>
    /// <param name="repetitions">Quante volte ripetere la misura all'interno di ogni singolo trial.</param>
    /// <param name="mutatesInput">
    /// True se <paramref name="run"/> modifica l'array (es. un sort): in tal caso serve un array
    /// fresco a ogni ripetizione. False se <paramref name="run"/> si limita a leggere l'array
    /// (es. una ricerca): in tal caso lo stesso array viene riusato per tutte le ripetizioni,
    /// evitando di allocare "repetitions" copie dell'input in memoria.
    /// </param>
    /// <param name="trials">
    /// Quante volte ripetere l'intera batch di misure per ogni dimensione. Si usa la mediana
    /// dei trial anziché la media, così un singolo trial disturbato (pausa del garbage
    /// collector, interrupt del sistema operativo, ecc.) non falsa la stima.
    /// </param>
    internal static void Analyze(string label, Func<int, int[]> arrayFactory, Action<int[]> run, int[] sizes, int repetitions = 5, bool mutatesInput = true, int trials = 5)
    {
        var measured = new List<(int N, double Ms)>();

        foreach (int n in sizes)
        {
            run(arrayFactory(n)); // riscaldamento per escludere il costo del JIT

            double[] trialAverages = new double[trials];
            int[]? sharedInput = mutatesInput ? null : arrayFactory(n);

            for (int t = 0; t < trials; t++)
            {
                Stopwatch stopwatch;
                if (mutatesInput)
                {
                    // Ogni ripetizione richiede un array fresco, generato PRIMA di avviare il
                    // cronometro così il costo di preparazione non entra nella misura.
                    int[][] inputs = new int[repetitions][];
                    for (int r = 0; r < repetitions; r++)
                    {
                        inputs[r] = arrayFactory(n);
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    stopwatch = Stopwatch.StartNew();
                    for (int r = 0; r < repetitions; r++)
                    {
                        run(inputs[r]);
                    }
                    stopwatch.Stop();
                }
                else
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    stopwatch = Stopwatch.StartNew();
                    for (int r = 0; r < repetitions; r++)
                    {
                        run(sharedInput!);
                    }
                    stopwatch.Stop();
                }

                trialAverages[t] = stopwatch.Elapsed.TotalMilliseconds / repetitions;
            }

            measured.Add((n, Median(trialAverages)));
        }

        string best = "";
        double bestR2 = double.NegativeInfinity;
        foreach (var (name, f) in Models)
        {
            double r2 = RSquared(measured, f);
            if (r2 > bestR2)
            {
                bestR2 = r2;
                best = name;
            }
        }

        Console.WriteLine($"{label}: {best}");
    }

    private static double Median(double[] values)
    {
        double[] sorted = (double[])values.Clone();
        Array.Sort(sorted);
        int mid = sorted.Length / 2;
        return sorted.Length % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2.0
            : sorted[mid];
    }

    /// <summary>Coefficiente di determinazione (R²) tra i tempi misurati e il modello f(n).</summary>
    private static double RSquared(List<(int N, double Ms)> data, Func<double, double> f)
    {
        double[] x = data.Select(d => f(d.N)).ToArray();
        double[] y = data.Select(d => d.Ms).ToArray();

        double xMean = x.Average();
        double yMean = y.Average();

        double num = 0, denX = 0, denY = 0;
        for (int i = 0; i < x.Length; i++)
        {
            double dx = x[i] - xMean;
            double dy = y[i] - yMean;
            num += dx * dy;
            denX += dx * dx;
            denY += dy * dy;
        }

        if (denX <= 0 || denY <= 0)
        {
            return 0;
        }

        double r = num / Math.Sqrt(denX * denY);
        return r * r;
    }
}
