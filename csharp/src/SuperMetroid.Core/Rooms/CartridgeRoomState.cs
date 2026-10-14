namespace SuperMetroid.Core.Rooms;

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
    SuperMetroid.Core.Game.Layer2ScrollModes Layer2Scroll,
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
