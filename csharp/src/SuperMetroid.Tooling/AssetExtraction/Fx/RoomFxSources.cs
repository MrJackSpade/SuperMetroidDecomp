using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import pipeline's map between the bank-$83 FX header pointers room states store and the compiled FX headers
/// they address; the simulation knows only the named headers.
/// </summary>
public static class RoomFxSources
{
    /// <summary>Every compiled header pointer, in ascending order.</summary>
    public static IEnumerable<ushort> Pointers => RoomFxSourceDefinitions.ByPointer.Keys.Order();

    /// <summary>The compiled FX header at <paramref name="pointer"/>.</summary>
    public static RoomFxDefinition Fx(ushort pointer) =>
        RoomFxSourceDefinitions.ByPointer.TryGetValue(pointer, out RoomFxDefinition? found)
            ? found
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not one of the 285 retail room FX headers.");
}
