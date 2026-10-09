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
/// <param name="Pointer">Bank-$8F address of the selected room's fixed header.</param>
/// <param name="RoomIndex">Room number within <paramref name="AreaIndex"/>.</param>
/// <param name="AreaIndex">Area containing the room.</param>
/// <param name="MapX">Room's horizontal origin on the area map.</param>
/// <param name="MapY">Room's vertical origin on the area map.</param>
/// <param name="WidthInScreens">Room width in SNES screens, as stored in the fixed header.</param>
/// <param name="HeightInScreens">Room height in SNES screens, as stored in the fixed header.</param>
/// <param name="UpScroller">Cartridge up-scroller setting for this room.</param>
/// <param name="DownScroller">Cartridge down-scroller setting for this room.</param>
/// <param name="CreBitset">CRE bitset copied from the room header.</param>
/// <param name="DoorListPointer">Bank-relative pointer to the room's door list.</param>
/// <param name="State">State payload selected from this room's state table.</param>
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
/// <param name="Pointer">Bank-$8F address of the selected room-state header.</param>
/// <param name="CompressedLevelDataAddress">Address of the compressed level block stream.</param>
/// <param name="GraphicsSet">Graphics-set index loaded for this state.</param>
/// <param name="MusicDataIndex">Music data-set index requested by the state.</param>
/// <param name="MusicTrackIndex">Track index within the selected music data set.</param>
/// <param name="FxPointer">Pointer to the room's environmental-effects definition.</param>
/// <param name="EnemyPopulationPointer">Pointer to the enemy population list.</param>
/// <param name="EnemyTilesetPointer">Pointer to the enemy graphics tileset data.</param>
/// <param name="Layer2ScrollX">Initial horizontal layer-two scroll mode.</param>
/// <param name="Layer2ScrollY">Initial vertical layer-two scroll mode.</param>
/// <param name="ScrollPointer">Pointer to the room's scroll data.</param>
/// <param name="XrayPointer">Pointer to the state's X-Ray block-reveal data.</param>
/// <param name="MainCodePointer">Native callback pointer run by the room main loop.</param>
/// <param name="PlmPointer">Pointer to the state's PLM population list.</param>
/// <param name="BackgroundDataPointer">Pointer to the background data loaded for this state.</param>
/// <param name="SetupCodePointer">Native callback pointer run during room setup.</param>
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
/// <param name="Events">Event bytes used by selector conditions; absent bytes are treated as clear.</param>
/// <param name="BossBits">Defeated-boss bits consulted by selector conditions.</param>
/// <param name="HasMorphBallAndMissiles">Whether both Morph Ball and Missiles are present.</param>
/// <param name="HasPowerBombs">Whether Power Bombs are present.</param>
public readonly record struct RoomStateSelectionContext(
    ReadOnlyMemory<byte> Events,
    BossBits BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    /// <summary>Tests one native event bit, treating indices beyond the supplied event memory as clear.</summary>
    /// <param name="eventIndex">Zero-based bit index into the event byte sequence.</param>
    /// <returns>True when the corresponding supplied bit is set; false when clear or absent.</returns>
    public bool IsEventSet(byte eventIndex)
    {
        ReadOnlySpan<byte> events = Events.Span;
        int byteIndex = eventIndex >> 3;
        return byteIndex < events.Length && (events[byteIndex] & (1 << (eventIndex & 7))) != 0;
    }

    /// <summary>Tests whether the selected area's boss-state word contains any bit from a validated known mask.</summary>
    /// <param name="mask">A mask composed only of established <see cref="BossBits"/> values; zero is allowed.</param>
    /// <returns>True when at least one selected boss-defeat bit is set; a zero mask returns false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The mask contains an unproven boss-state bit.</exception>
    public bool IsBossDead(BossBits mask)
    {
        BossBitMasks.Validate(mask, "Room-state boss selector");
        return (BossBits & mask) != 0;
    }
}
