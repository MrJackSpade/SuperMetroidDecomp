using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomStateSources"/> that only verification reads.</summary>
internal static class RoomStateSourcesConstants
{
    /// <summary>Each compiled state's pointer, by the state's name.</summary>
    internal static readonly Dictionary<string, ushort> PointersByName =
        RoomStateSourceDefinitions.ByPointer.ToDictionary(pair => pair.Value.Name, pair => pair.Key, StringComparer.Ordinal);
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
