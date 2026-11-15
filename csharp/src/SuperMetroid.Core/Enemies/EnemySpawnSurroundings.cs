using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a room's population load hands the enemies it spawns: the room's level data, Samus, and the frame's controller
/// input and camera. An enemy spawned on another enemy's behalf has <see cref="None"/>: no level and no Samus. A
/// population load has no Samus only while a new game loads Ceres's first room, before Samus is created.
/// </summary>
/// <param name="Level">The room's level data, or null for a spawn on another enemy's behalf.</param>
/// <param name="Samus">Samus, or null for a spawn on another enemy's behalf or before Samus is created.</param>
/// <param name="ControllerInput">The frame's controller input.</param>
/// <param name="Camera">The destination camera's position.</param>
[RoomScopeBoundary("A per-call input, built for one population load and never kept.")]
internal sealed record EnemySpawnSurroundings(RoomLevelData? Level, SamusState? Samus, GameplayAction ControllerInput, Position16<RoomPixels> Camera)
{
    /// <summary>The surroundings of a spawn outside a population load.</summary>
    public static EnemySpawnSurroundings None { get; } = new(null, null, GameplayAction.None, default);

    /// <summary>The room's level data, which only a population load hands its enemies.</summary>
    /// <exception cref="InvalidOperationException">The spawn is on another enemy's behalf.</exception>
    public RoomLevelData PopulationLevel => Level ?? throw new InvalidOperationException(
        "An enemy whose initialization reads the room's level was spawned on another enemy's behalf.");

    /// <summary>Samus, as a population load after her creation hands her to its enemies.</summary>
    /// <exception cref="InvalidOperationException">The spawn has no Samus.</exception>
    public SamusState IntroducedSamus => Samus ?? throw new InvalidOperationException(
        "An enemy whose initialization reads Samus was spawned without her: on another enemy's behalf, or before she was created.");
}
