namespace BigOAnalyzerApp;

internal static class Algorithms
{
    internal static void insertionSort(int[] arr)
    {
        for (int i = 1; i < arr.Length; i++)
        {
            int key = arr[i];
            int j = i - 1;

            while (j >= 0 && arr[j] > key)
            {
                arr[j + 1] = arr[j];
                j--;
            }
            arr[j + 1] = key;
        }
    }

    internal static void mergeSort(int[] arr)
    {
        if (arr.Length < 2)
        {
            return;
        }

        int mid = arr.Length / 2;
        int[] left = arr[..mid];
        int[] right = arr[mid..];

        mergeSort(left);
        mergeSort(right);

        int i = 0, j = 0, k = 0;
        while (i < left.Length && j < right.Length)
        {
            arr[k++] = left[i] <= right[j] ? left[i++] : right[j++];
        }
        while (i < left.Length)
        {
            arr[k++] = left[i++];
        }
        while (j < right.Length)
        {
            arr[k++] = right[j++];
        }
    }

    internal static bool HasTripletSum(int[] arr, int target)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            for (int j = i + 1; j < arr.Length; j++)
            {
                for (int k = j + 1; k < arr.Length; k++)
                {
                    if (arr[i] + arr[j] + arr[k] == target)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    internal static int LinSrc(int[] arr, int n)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == n)
            {
                return i;
            }
        }
        return -1;
    }

    internal static int BinSrc(int[] arr, int n)
    {
        int left = 0;
        int right = arr.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (arr[mid] == n)
            {
                return mid;
            }
            else if (arr[mid] < n)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return -1;
    }
}
