
namespace SmartX.Api.Services;

/// Stores sequential historical batches of raw telemetry arrays using a
/// jagged array (float[][]) before transferring them into List<T> collections and float[][] allows each batch to have a different length ,realistic for IoT
/// (IIE, 2026).

public class TelemetryBatchStore
{
    // Jagged arrays: array of arrays (Microsoft Docs, 2026).
    private readonly float[][] _moistureBatches;
    private readonly int[][] _wattageBatches;
    private readonly bool[][] _valveBatches;

    public TelemetryBatchStore(int numberOfBatches, int batchSize)
    {
        // Initialise outer array (Microsoft Docs, 2026).
        _moistureBatches = new float[numberOfBatches][];
        _wattageBatches = new int[numberOfBatches][];
        _valveBatches = new bool[numberOfBatches][];

        // Initialise each inner array separately (Sedgewick and Wayne, 2011).
        for (int i = 0; i < numberOfBatches; i++)
        {
            _moistureBatches[i] = new float[batchSize];
            _wattageBatches[i] = new int[batchSize];
            _valveBatches[i] = new bool[batchSize];
        }
    }

    // Array.Copy for efficient bulk memory transfer (Microsoft Docs, 2026).
    public void StoreMoistureBatch(int batchIndex, float[] values)
    {
        if (batchIndex >= 0 && batchIndex < _moistureBatches.Length)
        {
            Array.Copy(values, _moistureBatches[batchIndex],
                       Math.Min(values.Length, _moistureBatches[batchIndex].Length));
        }
    }

    public void StoreWattageBatch(int batchIndex, int[] values)
    {
        if (batchIndex >= 0 && batchIndex < _wattageBatches.Length)
        {
            Array.Copy(values, _wattageBatches[batchIndex],
                       Math.Min(values.Length, _wattageBatches[batchIndex].Length));
        }
    }

    // Transfer jagged array contents into List<T> for downstream processing
    // (IIE, 2026).
    public List<float> GetAllMoistureReadings()
    {
        var result = new List<float>();
        foreach (var batch in _moistureBatches)
        {
            result.AddRange(batch);
        }
        return result;
    }

    public List<int> GetAllWattageReadings()
    {
        var result = new List<int>();
        foreach (var batch in _wattageBatches)
        {
            result.AddRange(batch);
        }
        return result;
    }
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Jagged arrays (C# programming guide). [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/jagged-arrays> [Accessed 12 September 2026].

Microsoft Docs, 2026. Array.Copy method. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.array.copy> [Accessed 12 September 2026].

Sedgewick, R. and Wayne, K., 2011. Algorithms. 4th ed. Boston: Addison-Wesley.
*/