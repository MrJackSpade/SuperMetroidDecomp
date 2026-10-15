using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Ordered bank-$8F scroll mutations selected by retail trigger identity.</summary>
internal static partial class RoomPlmScrollProgramDefinitions
{

    /// <summary>Applies the retail trigger's ordered scroll writes and rejects an unknown bank-$8F pointer.</summary>
    internal static void Apply(ushort pointer, Action<int, RoomScrollState> write)
    {
        Ensure.NotNull(write);
        if (!TryApply(pointer, write))
            throw new InvalidDataException($"Compiled room scroll programs lack retail source $8F:{pointer:X4}.");
    }
}
