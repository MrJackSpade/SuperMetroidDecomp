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
sealed class TestAddressSpace : ISnesAddressSpace, ISnesMutableMemory,
    IImportCartridgeSource,
    IRoomEnemyFixtureSource, IRoomEnemyFallingTubeFixtureSource
{
    private readonly Dictionary<int, byte> _bytes = [];

    internal bool HasSeededByte(int address) => _bytes.ContainsKey(address);

    public byte ReadByte(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        return _bytes.GetValueOrDefault(address);
    }

    public byte ReadCartridgeByte(int address)
    {
        SnesAddress source = SnesAddress.FromBusAddress(address);
        if (!source.IsUpperLoRomWindow || source.Bank is 0x7e or 0x7f)
            throw new ArgumentOutOfRangeException(nameof(address), address,
                "Synthetic cartridge bytes require an upper LoROM address.");
        return ReadByte(address);
    }

    // This sparse fixture stores exactly the CPU addresses written by its test setup.
    // It deliberately does not synthesize bank mirrors; callers can assert that a
    // translated routine chooses the intended mutable alias itself.
    public byte ReadWorkRamByte(int address) => ReadByte(address);

    public byte ReadSaveRamByte(int address) => ReadByte(address);

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

    public RoomEnemyDefinition ReadEnemyDefinition(ushort pointer) =>
        SuperMetroid.AssetExtraction.RoomEnemyDefinitionImporter.Load(this, pointer);


    public RoomEnemyPopulationRecord ReadFallingTubePopulation(ushort pointer)
    {
        int address = MotherBrainFallingTubePopulationDefinitions.NativeBank | pointer;
        ushort Word(int offset) => (ushort)(ReadByte(address + offset) |
            ReadByte(address + offset + 1) << 8);
        return new RoomEnemyPopulationRecord(
            Word(0), Word(2), Word(4), Word(6), Word(8), Word(10),
            Word(12), Word(14));
    }

    public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer)
    {
        int cursor = RoomEnemyRomLayout.PopulationBank | pointer;
        var records = new List<RoomEnemyPopulationRecord>();
        for (int slot = 0; slot <= RoomEnemySystem.MaximumEnemyCount; slot++)
        {
            ushort definitionPointer = ReadFixtureWord(cursor);
            if (definitionPointer == 0xffff)
                return new RoomEnemyPopulationDefinition(
                    pointer, records.ToArray(), ReadByte(cursor + 2));
            if (slot == RoomEnemySystem.MaximumEnemyCount)
                throw new InvalidDataException($"Fixture population ${pointer:X4} exceeds 32 entries.");
            records.Add(new RoomEnemyPopulationRecord(
                definitionPointer, ReadFixtureWord(cursor + 2),
                ReadFixtureWord(cursor + 4), ReadFixtureWord(cursor + 6),
                ReadFixtureWord(cursor + 8), ReadFixtureWord(cursor + 10),
                ReadFixtureWord(cursor + 12), ReadFixtureWord(cursor + 14)));
            cursor += 16;
        }
        throw new InvalidDataException($"Fixture population ${pointer:X4} is unterminated.");
    }

    public RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer)
    {
        int cursor = RoomEnemyRomLayout.TilesetBank | pointer;
        var records = new List<RoomEnemyGraphicsSetHeader>();
        for (int slot = 0; slot <= 4; slot++)
        {
            ushort definitionPointer = ReadFixtureWord(cursor);
            if (definitionPointer == 0xffff)
                return new RoomEnemyGraphicsSetDefinition(pointer, records.ToArray());
            if (slot == 4)
                throw new InvalidDataException($"Fixture graphics set ${pointer:X4} exceeds four entries.");
            records.Add(new RoomEnemyGraphicsSetHeader(
                definitionPointer, ReadFixtureWord(cursor + 2)));
            cursor += 4;
        }
        throw new InvalidDataException($"Fixture graphics set ${pointer:X4} is unterminated.");
    }

    private ushort ReadFixtureWord(int address) =>
        unchecked((ushort)(ReadByte(address) | ReadByte(address + 1) << 8));
}

/// <summary>
/// Windows process policy used only by this console host. `SEM_FAILCRITICALERRORS`,
/// `SEM_NOGPFAULTERRORBOX`, and `SEM_NOOPENFILEERRORBOX` keep failures non-interactive;
/// .NET still writes the exception and stack trace to stderr and returns a failing code.
/// </summary>
static partial class NativeConsoleProcess
{
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
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

    /// <summary>Compares a color with an expected cartridge word, decoded as a 15-bit color.</summary>
    internal static void AssertEqual(int expectedWord, Bgr555 actual, string context) =>
        AssertEqual(Bgr555.FromWord(checked((ushort)expectedWord)), actual, context);

    /// <summary>Compares an expected color with an actual cartridge word, decoded as a 15-bit color.</summary>
    internal static void AssertEqual(Bgr555 expected, ushort actualWord, string context) =>
        AssertEqual(expected, Bgr555.FromWord(actualWord), context);

    /// <summary>Compares a nullable color with an expected cartridge word.</summary>
    internal static void AssertEqual(int expectedWord, Bgr555? actual, string context) =>
        AssertEqual<Bgr555?>(Bgr555.FromWord(checked((ushort)expectedWord)), actual, context);

    internal static TException AssertThrows<TException>(Action action, string context)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException(
            $"Verification failed: {context}; expected {typeof(TException).Name}.");
    }
}
