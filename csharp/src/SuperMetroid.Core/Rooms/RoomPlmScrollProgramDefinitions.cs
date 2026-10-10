using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Ordered bank-$8F scroll mutations selected by retail trigger identity.</summary>
internal static partial class RoomPlmScrollProgramDefinitions
{
    /// <summary>Applies the compiled ordered scroll-state writes for a retail PLM source pointer.</summary>
    /// <param name="pointer">Bank-$8F address identifying the scroll program.</param>
    /// <param name="write">Receives each storage index and state in the program's original write order.</param>
    /// <exception cref="ArgumentNullException"><paramref name="write"/> is null.</exception>
    /// <exception cref="InvalidDataException">No compiled scroll program exists for <paramref name="pointer"/>.</exception>
    internal static void Apply(ushort pointer, Action<int, RoomScrollState> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        if (!TryApply(pointer, write))
            throw new InvalidDataException($"Compiled room scroll programs lack retail source $8F:{pointer:X4}.");
    }
}
