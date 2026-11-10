using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The settings one state of a room loads with: its level, graphics set, music, effects, enemies, scrolling, X-ray
/// overlay, room callbacks, room objects and background, as loading the state copies them.
/// </summary>
public sealed record RoomStateSettings(
    string Name,
    RoomLevelId Level,
    int GraphicsSet,
    RoomMusic Music,
    SuperMetroid.Core.Fx.RoomFxHeader? Fx,
    Enemies.RoomEnemyPopulationDefinition EnemyPopulation,
    SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes BackgroundScrollModes,
    SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinition Scroll,
    SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinition? XrayOverlay,
    RoomMainCallback MainCallback,
    SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation ObjectPopulation,
    SuperMetroid.Core.Rooms.Backgrounds.LibraryBackground? Background,
    RoomSetupCallback SetupCallback)
{
    /// <summary>The state's name.</summary>
    public string Name { get; init; } = Ensure.NotNull(Name);

    /// <summary>The level the state loads: its initial blocks, block types and background blocks, shared with the room's other states that load the same one.</summary>
    public RoomLevelId Level { get; init; } = Level;

    /// <summary>The room state's graphics theme, 0 through 255.</summary>
    public int GraphicsSet { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, GraphicsSet);

    /// <summary>The music data set and track the room state selects.</summary>
    public RoomMusic Music { get; init; } = Music;
}
