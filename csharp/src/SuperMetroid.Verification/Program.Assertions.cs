using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Reference arithmetic, assertions, sparse bus fixture, and console process policy.</summary>
static ushort ReferenceNextRandom(ushort seed)
{
    int hardwareProductLow = (seed & 0xff) * 5;
    byte stackLow = (byte)hardwareProductLow;
    byte stackHigh = (byte)(hardwareProductLow >> 8);

    int hardwareProductHigh = ((seed >> 8) & 0xff) * 5;
    int eightBitAdc = stackHigh + (hardwareProductHigh & 0xff) + 1;
    stackHigh = (byte)eightBitAdc;
    int carry = eightBitAdc > 0xff ? 1 : 0;

    int restoredAccumulator = stackLow | (stackHigh << 8);
    return unchecked((ushort)(restoredAccumulator + 0x0011 + carry));
}

/// <summary>
/// Sparse CPU-bus fixture. Unwritten addresses read as zero, mirroring cleared memory and
/// making every byte relevant to a transfer visible in the setup directly above it.
/// </summary>
sealed class TestAddressSpace : ISnesAddressSpace
{
    private readonly Dictionary<int, byte> _bytes = [];

    public byte ReadByte(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        return _bytes.GetValueOrDefault(address);
    }

    public void WriteByte(int address, byte value)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        _bytes[address] = value;
    }

    public void WriteBytes(int startAddress, ReadOnlySpan<byte> values)
    {
        for (int index = 0; index < values.Length; index++)
            WriteByte(startAddress + index, values[index]);
    }
}

/// <summary>
/// Windows process policy used only by this console host. `SEM_FAILCRITICALERRORS`,
/// `SEM_NOGPFAULTERRORBOX`, and `SEM_NOOPENFILEERRORBOX` keep failures non-interactive;
/// .NET still writes the exception and stack trace to stderr and returns a failing code.
/// </summary>
static class NativeConsoleProcess
{
    [DllImport("kernel32.dll")]
    internal static extern uint SetErrorMode(uint errorMode);
}
}

/// <summary>
/// Dependency-free assertions shared by every verifier source file.
/// </summary>
/// <remarks>
/// Native-state properties are intentionally narrow (<see cref="byte"/>,
/// <see cref="ushort"/>, and <see cref="short"/>), while C# numeric literals and loop
/// expressions naturally begin as <see cref="int"/>. The numeric overloads centralize
/// that checked narrowing so tests can say <c>AssertEqual(0x8000, state.Word, ...)</c>
/// without repeating casts at hundreds of call sites. An out-of-range expectation fails
/// as bad fixture data instead of silently wrapping before the comparison.
/// </remarks>
internal static class VerificationAssert
{
    internal static void AssertTrue(bool condition, string context)
    {
        if (!condition)
            throw new InvalidOperationException($"Verification failed: {context}.");
    }

    internal static void AssertEqual<T>(T expected, T actual, string context)
    {
        // EqualityComparer<T>.Default handles primitives, records, nullable values, and
        // enums. Constraining T to IEquatable<T> looks attractive but excludes C# enums.
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                $"Verification failed: {context}; expected {expected}, got {actual}.");
        }
    }

    internal static void AssertEqual(int expected, byte actual, string context) =>
        AssertEqual<byte>(checked((byte)expected), actual, context);

    internal static void AssertEqual(int expected, sbyte actual, string context) =>
        AssertEqual<sbyte>(checked((sbyte)expected), actual, context);

    internal static void AssertEqual(int expected, ushort actual, string context) =>
        AssertEqual<ushort>(checked((ushort)expected), actual, context);

    internal static void AssertEqual(int expected, short actual, string context) =>
        AssertEqual<short>(checked((short)expected), actual, context);

    internal static void AssertEqual(int expected, ushort? actual, string context) =>
        AssertEqual<ushort?>(checked((ushort)expected), actual, context);

    internal static void AssertThrows<TException>(Action action, string context)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Verification failed: {context}; expected {typeof(TException).Name}.");
    }
}
