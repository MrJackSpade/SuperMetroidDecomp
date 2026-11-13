using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The settings one state of a room loads with: its level, graphics set, music, effects, enemies, scrolling, X-ray
/// overlay, room behaviours, room objects and background, as loading the state copies them.
/// </summary>
public sealed record RoomStateSettings(
    string Name,
    RoomLevelId Level,
    RoomGraphicsSet GraphicsSet,
    RoomMusic Music,
    SuperMetroid.Core.Fx.RoomFxSelection? Fx,
    Enemies.RoomEnemyPopulationDefinition EnemyPopulation,
    SuperMetroid.Core.Rooms.Scrolling.BackgroundScrollModes BackgroundScrollModes,
    SuperMetroid.Core.Rooms.Scrolling.RoomScrollDefinition Scroll,
    SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinition? XrayOverlay,
    RoomMainBehavior MainBehavior,
    SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation ObjectPopulation,
    SuperMetroid.Core.Rooms.Backgrounds.LibraryBackground? Background,
    RoomSetupBehavior SetupBehavior)
{
    /// <summary>The state's name.</summary>
    public string Name { get; init; } = Ensure.NotNull(Name);

    /// <summary>The level the state loads: its initial blocks, block types and background blocks, shared with the room's other states that load the same one.</summary>
    public RoomLevelId Level { get; init; } = Level;

    /// <summary>The graphics set the state draws its rooms with.</summary>
    public RoomGraphicsSet GraphicsSet { get; init; } = GraphicsSet;

    /// <summary>The music data set and track the room state selects.</summary>
    public RoomMusic Music { get; init; } = Music;
}
