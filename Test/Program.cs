namespace MioProgetto;

internal class Program
{
    static void Main(string[] args)
    {
        // insertionSort: caso peggiore = array in ordine decrescente. Il sort modifica
        // l'array, quindi ogni ripetizione richiede una copia fresca dell'input.
        BigOAnalyzer.Analyze(
            "insertionSort",
            size =>
            {
                int[] a = new int[size];
                for (int i = 0; i < size; i++)
                {
                    a[i] = size - i;
                }
                return a;
            },
            Algorithms.insertionSort,
            sizes: [1_000, 2_000, 4_000, 8_000, 16_000],
            repetitions: 1,
            mutatesInput: true,
            trials: 5);

        // mergeSort: il tempo non dipende dall'ordine iniziale, quindi si usa lo stesso
        // generatore di insertionSort. Il sort modifica l'array, quindi ogni ripetizione
        // richiede una copia fresca dell'input. Range ampio (256x) per distinguere bene
        // O(n log n) da O(n): su un range troppo stretto le due curve sono troppo simili.
        BigOAnalyzer.Analyze(
            "mergeSort",
            size =>
            {
                int[] a = new int[size];
                for (int i = 0; i < size; i++)
                {
                    a[i] = size - i;
                }
                return a;
            },
            Algorithms.mergeSort,
            sizes: [20_000, 80_000, 320_000, 1_280_000, 5_120_000],
            repetitions: 1,
            mutatesInput: true,
            trials: 5);

        // HasTripletSum: caso peggiore = nessuna tripla la cui somma è il target
        // (vengono esaminate tutte le O(n^3) combinazioni). Non modifica l'array.
        BigOAnalyzer.Analyze(
            "HasTripletSum",
            BuildSortedArray,
            a => Algorithms.HasTripletSum(a, int.MinValue),
            sizes: [50, 100, 150, 200, 250],
            repetitions: 20,
            mutatesInput: false,
            trials: 5);

        // LinSrc: caso peggiore = elemento non presente nell'array (scansione completa).
        // Non modifica l'array: lo stesso input viene riusato per tutte le ripetizioni.
        BigOAnalyzer.Analyze(
            "LinSrc",
            BuildSortedArray,
            a => Algorithms.LinSrc(a, -1),
            sizes: [200_000, 400_000, 800_000, 1_600_000, 3_200_000],
            repetitions: 30,
            mutatesInput: false,
            trials: 5);

        // BinSrc: caso peggiore = elemento non presente nell'array.
        BigOAnalyzer.Analyze(
            "BinSrc",
            BuildSortedArray,
            a => Algorithms.BinSrc(a, -1),
            sizes: [200_000, 400_000, 800_000, 1_600_000, 3_200_000],
            repetitions: 50_000,
            mutatesInput: false,
            trials: 5);
    }

    private static int[] BuildSortedArray(int size)
    {
        int[] a = new int[size];
        for (int i = 0; i < size; i++)
        {
            a[i] = i + 1;
        }
        return a;
    }

}

