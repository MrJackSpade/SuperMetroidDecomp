using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Lossless cartridge view of bank $8F's eleven-byte room header and its selected
/// twenty-six-byte room-state record.
/// </summary>
/// <remarks>
/// The engine calls <c>HandleRoomDefStateSelect</c> at $8F:E5D2 immediately after reading
/// the fixed header. State selection is data-driven and can depend on events, bosses, or
/// inventory. This first reusable loader accepts those facts explicitly; a fresh Ceres
/// start supplies an all-clear context and therefore follows the exact default branch.
/// </remarks>
public sealed record CartridgeRoomHeader(
    ushort Pointer,
    byte RoomIndex,
    AreaId AreaIndex,
    byte MapX,
    byte MapY,
    byte WidthInScreens,
    byte HeightInScreens,
    byte UpScroller,
    byte DownScroller,
    byte CreBitset,
    ushort DoorListPointer,
    CartridgeRoomState State)
{
    /// <summary>Logical area/room pair, kept separate from <see cref="Pointer"/>.</summary>
    public RoomIdentity Identity => new(AreaIndex, RoomIndex);

    /// <summary>Builds a room entirely from compiled fixed metadata, selection, and state payloads.</summary>
    public static CartridgeRoomHeader LoadUsingCompiledSelection(
        ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        RoomHeaderDefinition header = RoomHeaderDefinitions.Get(roomPointer);
        ushort statePointer = RoomStateSelectionDefinitions.Select(roomPointer, selection);
        return new CartridgeRoomHeader(
            header.Pointer,
            header.RoomIndex,
            header.AreaIndex,
            header.MapX,
            header.MapY,
            header.WidthInScreens,
            header.HeightInScreens,
            header.UpScroller,
            header.DownScroller,
            header.CreBitset,
            header.DoorListPointer,
            RoomStateDefinitions.Get(statePointer));
    }

    /// <summary>Returns a compiled fixed header's area without reading native room data.</summary>
    public static AreaId ReadAreaIndex(ushort roomPointer) =>
        RoomHeaderDefinitions.Get(roomPointer).AreaIndex;

}

/// <summary>The fields copied by <c>LoadStateHeader</c> at $82:DEF2.</summary>
public sealed record CartridgeRoomState(
    ushort Pointer,
    int CompressedLevelDataAddress,
    byte GraphicsSet,
    byte MusicDataIndex,
    byte MusicTrackIndex,
    ushort FxPointer,
    ushort EnemyPopulationPointer,
    ushort EnemyTilesetPointer,
    byte Layer2ScrollX,
    byte Layer2ScrollY,
    ushort ScrollPointer,
    ushort XrayPointer,
    ushort MainCodePointer,
    ushort PlmPointer,
    ushort BackgroundDataPointer,
    ushort SetupCodePointer)
{
    /// <summary>Typed mutually exclusive identity of the selected room-main callback.</summary>
    public RoomMainCallback MainCallback =>
        RoomCallbackDefinitions.ResolveMain(MainCodePointer);

    /// <summary>Typed mutually exclusive identity of the selected room-setup callback.</summary>
    public RoomSetupCallback SetupCallback =>
        RoomCallbackDefinitions.ResolveSetup(SetupCodePointer);

}

/// <summary>Facts inspected by the cartridge's room-state selector functions.</summary>
public readonly record struct RoomStateSelectionContext(
    ReadOnlyMemory<byte> Events,
    BossBits BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    public bool IsEventSet(byte eventIndex)
    {
        ReadOnlySpan<byte> events = Events.Span;
        int byteIndex = eventIndex >> 3;
        return byteIndex < events.Length && (events[byteIndex] & (1 << (eventIndex & 7))) != 0;
    }

    public bool IsBossDead(BossBits mask)
    {
        BossBitMasks.Validate(mask, "Room-state boss selector");
        return (BossBits & mask) != 0;
    }
}
