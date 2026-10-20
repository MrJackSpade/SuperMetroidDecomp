namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between bank-$8F room-state pointers and the compiled states they address; the
/// simulation knows only the named states.
/// </summary>
public static class RoomStateSources
{

    /// <summary>Every compiled state pointer, in ascending order.</summary>
    public static IEnumerable<ushort> Pointers => RoomStateSourceDefinitions.ByPointer.Keys.Order();
}
