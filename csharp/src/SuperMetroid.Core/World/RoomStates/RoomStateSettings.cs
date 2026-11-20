using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The settings one state of a room loads with: its level, graphics set, music, effects, enemies, scrolling, X-ray
/// overlay, room behaviours, room objects and background, as loading the state copies them.
/// </summary>
[ArchitectureReview("context-bag", "A room state's authored definition data; its setup and main members are immutable behaviour definitions the room load composes (#2287), not services carried through.")]
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

    /// <summary>
    /// What the state's load sets up besides its definition (#2287), checked once against the state's effects as the state
    /// is defined (#2554).
    /// </summary>
    public RoomSetupBehavior SetupBehavior { get; init; } = RoomSetupBehavior.Checked(SetupBehavior, Name, Fx);

    /// <summary>The level the state loads: its initial blocks, block types and background blocks, shared with the room's other states that load the same one.</summary>
    public RoomLevelId Level { get; init; } = Level;

    /// <summary>The graphics set the state draws its rooms with.</summary>
    public RoomGraphicsSet GraphicsSet { get; init; } = GraphicsSet;

    /// <summary>The music data set and track the room state selects.</summary>
    public RoomMusic Music { get; init; } = Music;

    /// <summary>
    /// The turning floor the state shows (#1649, #2287), authored on the state for the import that installs floors and the
    /// room load that binds one alike: the elevator shaft's states show the shaft's floor, whose screen is that floor alone
    /// (#1647); the Ceres Ridley fight's state shows the floor his getaway flies over; every other state shows none.
    /// </summary>
    public RoomTurningFloor TurningFloor { get; init; } = RoomTurningFloor.None;
}
