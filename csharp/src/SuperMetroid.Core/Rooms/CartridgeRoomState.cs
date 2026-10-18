namespace SuperMetroid.Core.Rooms;

/// <summary>The fields copied by <c>LoadStateHeader</c> at $82:DEF2.</summary>
public sealed record CartridgeRoomState(
    ushort Pointer,
    int CompressedLevelDataAddress,
    int GraphicsSet,
    int MusicDataIndex,
    int MusicTrackIndex,
    ushort FxPointer,
    Enemies.RoomEnemyPopulationDefinition EnemyPopulation,
    Enemies.RoomEnemyGraphicsSetDefinition EnemyGraphicsSet,
    SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes Layer2Scroll,
    Scrolling.RoomScrollDefinition Scroll,
    ushort XrayPointer,
    RoomMainCallback MainCallback,
    ushort PlmPointer,
    ushort BackgroundDataPointer,
    RoomSetupCallback SetupCallback)
{
    /// <summary>The room state's graphics theme, 0 through 255.</summary>
    public int GraphicsSet { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, GraphicsSet);

    /// <summary>The music data set the room state selects, 0 through 255.</summary>
    public int MusicDataIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, MusicDataIndex);

    /// <summary>The music track the room state selects within its data set, 0 through 255.</summary>
    public int MusicTrackIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, MusicTrackIndex);
}
