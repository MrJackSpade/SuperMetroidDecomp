using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Botwoon;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// The codec of Botwoon's movement table at <c>$B3:A058-$B3:E14F</c> (#1473): two signed bytes per sample, X then Y.
/// A (-128, 0) sample separates the paths and opens and closes the table; the second path also ends early at a
/// (-128, -16) terminator followed by padding no walk reaches. The game holds typed paths of steps; this is the one
/// place the table's layout is known, for checks.
/// </summary>
[EncodingOwner("Botwoon movement samples")]
internal static class BotwoonPathCodec
{
    /// <summary>The table's first sample, the opening separator.</summary>
    public const int TableStart = 0xb3a058;

    /// <summary>One past the table's last sample, the closing separator.</summary>
    public const int TableEndExclusive = 0xb3e150;

    /// <summary>The bank the descriptor table's sixteen-bit sample pointers address.</summary>
    private const int PointerBank = 0xb30000;

    /// <summary>The bytes of one sample.</summary>
    public const int SampleBytes = 2;

    /// <summary>The X value that marks a separator or terminator sample.</summary>
    private const sbyte Marker = sbyte.MinValue;

    /// <summary>The raw signed sample at <paramref name="address"/>.</summary>
    public static Offset16<RoomPixels> Sample(IImportCartridgeSource cartridge, int address)
    {
        Ensure.NotNull(cartridge);
        return new(unchecked((sbyte)cartridge.ReadCartridgeByte(address)),
            unchecked((sbyte)cartridge.ReadCartridgeByte(address + 1)));
    }

    /// <summary>Whether the sample at <paramref name="address"/> ends a walk: a separator or the early terminator.</summary>
    public static bool IsMarker(IImportCartridgeSource cartridge, int address) =>
        Sample(cartridge, address).X == Marker;

    /// <summary>Whether the sample at <paramref name="address"/> separates two paths.</summary>
    private static bool IsSeparator(IImportCartridgeSource cartridge, int address) =>
        Sample(cartridge, address) == new Offset16<RoomPixels>(Marker, 0);

    /// <summary>The address of every separator, in table order; path <c>k</c> lies between separators <c>k</c> and <c>k + 1</c>.</summary>
    public static List<int> SeparatorAddresses(IImportCartridgeSource cartridge)
    {
        var separators = new List<int>();
        for (int address = TableStart; address < TableEndExclusive; address += SampleBytes)
        {
            if (IsSeparator(cartridge, address))
                separators.Add(address);
        }
        return separators;
    }

    /// <summary>Decodes every path: the steps from just after its opening separator up to its first marker sample.</summary>
    public static BotwoonPathStep[][] Paths(IImportCartridgeSource cartridge)
    {
        List<int> separators = SeparatorAddresses(cartridge);
        var paths = new BotwoonPathStep[separators.Count - 1][];
        for (int path = 0; path < paths.Length; path++)
        {
            var steps = new List<BotwoonPathStep>();
            for (int address = separators[path] + SampleBytes; !IsMarker(cartridge, address); address += SampleBytes)
                steps.Add(StepOf(Sample(cartridge, address)));
            paths[path] = [.. steps];
        }
        return paths;
    }

    /// <summary>
    /// The address of position <paramref name="step"/> of <paramref name="path"/>; position -1 is the path's opening
    /// separator and the position after its last step is its closing marker.
    /// </summary>
    /// <param name="separators">The table's separators, as <see cref="SeparatorAddresses"/> finds them.</param>
    /// <param name="path">The path.</param>
    /// <param name="step">The position within the path.</param>
    public static int AddressOf(IReadOnlyList<int> separators, int path, int step)
    {
        Ensure.NotNull(separators);
        return separators[path] + (step + 1) * SampleBytes;
    }

    /// <summary>The path and step a descriptor's sixteen-bit sample pointer <paramref name="pointer"/> addresses.</summary>
    /// <exception cref="InvalidDataException">The pointer addresses a separator or lies outside the table.</exception>
    public static BotwoonPathPosition Locate(IImportCartridgeSource cartridge, int pointer)
    {
        int address = PointerBank | pointer;
        if (address <= TableStart || address >= TableEndExclusive || IsSeparator(cartridge, address))
            throw new InvalidDataException($"Botwoon sample pointer ${pointer:X4} does not address a path step.");
        List<int> separators = SeparatorAddresses(cartridge);
        int path = separators.FindLastIndex(separator => separator < address);
        return new(path, (address - separators[path]) / SampleBytes - 1);
    }

    /// <summary>The step a one-pixel sample stands for.</summary>
    /// <exception cref="InvalidDataException">The sample is not a one-pixel step in one of the eight directions.</exception>
    public static BotwoonPathStep StepOf(Offset16<RoomPixels> sample) => sample switch
    {
        { X: 0, Y: -1 } => BotwoonPathStep.Up,
        { X: 1, Y: -1 } => BotwoonPathStep.UpRight,
        { X: 1, Y: 0 } => BotwoonPathStep.Right,
        { X: 1, Y: 1 } => BotwoonPathStep.DownRight,
        { X: 0, Y: 1 } => BotwoonPathStep.Down,
        { X: -1, Y: 1 } => BotwoonPathStep.DownLeft,
        { X: -1, Y: 0 } => BotwoonPathStep.Left,
        { X: -1, Y: -1 } => BotwoonPathStep.UpLeft,
        _ => throw new InvalidDataException($"Botwoon movement sample {sample} is not a one-pixel step."),
    };
}
