using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The fields copied by <c>LoadStateHeader</c> at $82:DEF2.</summary>
public sealed record CartridgeRoomState(
    string Name,
    int CompressedLevelDataAddress,
    int GraphicsSet,
    int MusicDataIndex,
    int MusicTrackIndex,
    SuperMetroid.Core.Fx.RoomFxDefinition? Fx,
    Enemies.RoomEnemyPopulationDefinition EnemyPopulation,
    Enemies.RoomEnemyGraphicsSetDefinition EnemyGraphicsSet,
    SuperMetroid.Core.Rooms.Scrolling.Layer2ScrollModes Layer2Scroll,
    SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinition Scroll,
    SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinition? XrayOverlay,
    RoomMainCallback MainCallback,
    SuperMetroid.Core.Rooms.Plms.RoomPlmPopulationDefinition PlmPopulation,
    SuperMetroid.Core.Rooms.Backgrounds.LibraryBackgroundProgram? Background,
    RoomSetupCallback SetupCallback)
{
    /// <summary>The state's name, the disassembly's label for it without its prefix.</summary>
    public string Name { get; init; } = Ensure.NotNull(Name);

    /// <summary>The room state's graphics theme, 0 through 255.</summary>
    public int GraphicsSet { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, GraphicsSet);

    /// <summary>The music data set the room state selects, 0 through 255.</summary>
    public int MusicDataIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, MusicDataIndex);

    /// <summary>The music track the room state selects within its data set, 0 through 255.</summary>
    public int MusicTrackIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, MusicTrackIndex);
}
