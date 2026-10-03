using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Ordered bank-$8F scroll mutations selected by retail trigger identity.</summary>
internal static partial class RoomPlmScrollProgramDefinitions
{
    internal const int RetailProgramCount = 173;
    internal const int RetailByteCount = 743;
    internal const int RetailPairCount = 285;

    internal static IEnumerable<ushort> Pointers
    {
        get
        {
            for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                if (TryApply((ushort)pointer, null)) yield return (ushort)pointer;
        }
    }

    /// <summary>Temporary native-format projection for verification/compatibility; gameplay executes writes directly.</summary>
    internal static ReadOnlyMemory<byte> Get(ushort pointer)
    {
        var bytes = new List<byte>();
        Apply(pointer, (index, state) => { bytes.Add((byte)index); bytes.Add((byte)state); });
        bytes.Add(0x80);
        return bytes.ToArray();
    }

    internal static void Apply(ushort pointer, Action<int, RoomScrollState> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        if (!TryApply(pointer, write))
            throw new InvalidDataException($"Compiled room scroll programs lack retail source $8F:{pointer:X4}.");
    }
}