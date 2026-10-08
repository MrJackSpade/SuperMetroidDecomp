using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Ordered bank-$8F scroll mutations selected by retail trigger identity.</summary>
internal static partial class RoomPlmScrollProgramDefinitions
{
    internal const int RetailProgramCount = 173;
    internal const int RetailByteCount = 743;
    internal const int RetailPairCount = 285;

    internal static void Apply(ushort pointer, Action<int, RoomScrollState> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        if (!TryApply(pointer, write))
            throw new InvalidDataException($"Compiled room scroll programs lack retail source $8F:{pointer:X4}.");
    }
}