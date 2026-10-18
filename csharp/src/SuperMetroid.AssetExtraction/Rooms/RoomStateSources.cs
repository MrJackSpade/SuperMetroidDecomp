using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between bank-$8F room-state pointers and the compiled states they address; the
/// simulation knows only the named states.
/// </summary>
public static class RoomStateSources
{
    /// <summary>Each compiled state's pointer, by the state's name.</summary>
    private static readonly Dictionary<string, ushort> PointersByName =
        RoomStateSourceDefinitions.ByPointer.ToDictionary(pair => pair.Value.Name, pair => pair.Key, StringComparer.Ordinal);

    /// <summary>Every compiled state pointer, in ascending order.</summary>
    public static IEnumerable<ushort> Pointers => RoomStateSourceDefinitions.ByPointer.Keys.Order();

    /// <summary>The compiled state at <paramref name="pointer"/>.</summary>
    public static CartridgeRoomState State(ushort pointer) =>
        RoomStateSourceDefinitions.ByPointer.TryGetValue(pointer, out CartridgeRoomState? found)
            ? found
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not one of the 323 selected retail room states.");

    /// <summary>The pointer of the compiled state <paramref name="state"/> is, or was imported as, by its name.</summary>
    public static ushort PointerOf(CartridgeRoomState state) =>
        PointersByName.TryGetValue(Ensure.NotNull(state).Name, out ushort pointer)
            ? pointer
            : throw new ArgumentOutOfRangeException(nameof(state), state.Name, "State is not one of the 323 retail room states.");
}
