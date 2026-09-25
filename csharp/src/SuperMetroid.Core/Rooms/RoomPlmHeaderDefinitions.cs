namespace SuperMetroid.Core.Rooms;

/// <summary>Native setup and first-instruction pointers for one retail room PLM header.</summary>
internal readonly record struct RoomPlmHeaderDefinition(
    ushort Header, ushort Setup, ushort InitialInstruction);

/// <summary>
/// Fixed bank-$84 header metadata selected by the compiled room PLM populations.
/// Setup dispatch and instruction execution remain in their owning systems.
/// </summary>
internal static partial class RoomPlmHeaderDefinitions
{
    internal const int RetailHeaderCount = 70;

    internal static ReadOnlySpan<RoomPlmHeaderDefinition> All => Sources;

    internal static RoomPlmHeaderDefinition Get(ushort header)
    {
        int low = 0;
        int high = Sources.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RoomPlmHeaderDefinition selected = Sources[middle];
            if (selected.Header == header)
                return selected;
            if (selected.Header < header)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Compiled room PLM headers lack retail header $84:{header:X4}.");
    }

    static RoomPlmHeaderDefinitions()
    {
        if (Sources.Length != RetailHeaderCount)
            throw new InvalidDataException("Compiled retail PLM header count changed.");
        for (int index = 1; index < Sources.Length; index++)
        {
            if (Sources[index - 1].Header >= Sources[index].Header)
                throw new InvalidDataException(
                    "Compiled retail PLM headers must be unique and sorted.");
        }
    }
}
