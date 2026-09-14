using System;

// Operator overloading for direct aggregation and delta comparison (Microsoft Docs, 2026).
// Struct used for small value types to reduce heap allocation (Albahari and Albahari, 2022).
// Rubric requirement: overloading +, -, and logical comparison operators (IIE, 2026).

namespace SmartX.Api.Models;

// Struct (not class) because sensor readings are small value types
public struct SensorReading
{
    public float Value { get; set; }
    public string SensorId { get; set; }
    public string Unit { get; set; }

    public SensorReading(float value, string sensorId, string unit = "")
    {
        Value = value;
        SensorId = sensorId;
        Unit = unit;
    }

    // OVERLOADED + : Aggregate load of two meters (Microsoft Docs, 2026)  
    // Example: Meter3 = Meter1 + Meter2
    public static SensorReading operator +(SensorReading a, SensorReading b)
    {
        return new SensorReading(
            a.Value + b.Value,
            $"{a.SensorId}+{b.SensorId}",
            a.Unit
        );
    }

    // OVERLOADED - : Delta between two readings (Microsoft Docs, 2026)  
    // Example: How much did power usage change?
    public static SensorReading operator -(SensorReading a, SensorReading b)
    {
        return new SensorReading(
            Math.Abs(a.Value - b.Value),
            $"{a.SensorId}-{b.SensorId}",
            a.Unit
        );
    }

    // OVERLOADED > and < : Threshold comparison (Microsoft Docs, 2026)  
    public static bool operator >(SensorReading a, float threshold)
        => a.Value > threshold;

    public static bool operator <(SensorReading a, float threshold)
        => a.Value < threshold;

    public static bool operator >(SensorReading a, SensorReading b)
        => a.Value > b.Value;

    public static bool operator <(SensorReading a, SensorReading b)
        => a.Value < b.Value;

    // OVERLOADED == and != : Equality comparison (Microsoft Docs, 2026)  
    // Floating-point tolerance used for equality (Albahari and Albahari, 2022).
    public static bool operator ==(SensorReading a, SensorReading b)
        => Math.Abs(a.Value - b.Value) < 0.001f;

    public static bool operator !=(SensorReading a, SensorReading b)
        => !(a == b);

    // Override Equals and GetHashCode to match == operator (Microsoft Docs, 2026).
    public override bool Equals(object? obj)
        => obj is SensorReading other && this == other;

    public override int GetHashCode()
        => HashCode.Combine(Value, SensorId);

    public override string ToString()
        => $"{SensorId}: {Value} {Unit}";
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Operator overloading (C# reference). [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading> [Accessed 12 September 2026].

Microsoft Docs, 2026. Equality operators (C# reference). [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/equality-operators> [Accessed 12 September 2026].
*/