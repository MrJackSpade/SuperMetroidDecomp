using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>A validated <see cref="IRoomEnemyLoad"/>; required capabilities are non-null at construction.</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
public sealed record RoomEnemyLoadRequest : IRoomEnemyLoad
{
    /// <summary>Creates a load of <paramref name="population"/> into the room whose load <paramref name="appearance"/> shows over.</summary>
    public RoomEnemyLoadRequest(RoomEnemyPopulationDefinition population, RoomAppearance appearance)
    {
        Population = Ensure.NotNull(population);
        Appearance = Ensure.NotNull(appearance);
    }

    /// <inheritdoc />
    public RoomEnemyPopulationDefinition Population { get; }

    /// <inheritdoc />
    public RoomAppearance Appearance { get; }

    /// <inheritdoc />
    public RoomLevelData? Level { get; init; }

    /// <inheritdoc />
    public SamusState? Samus { get; init; }

    /// <inheritdoc />
    public ControllerButton ControllerInput { get; init; }

    /// <inheritdoc />
    public Position16<RoomPixels> Camera { get; init; }

    /// <inheritdoc />
    public GunshipLoadScenario GunshipScenario { get; init; } = GunshipLoadScenario.Ordinary;

    /// <inheritdoc />
    public bool DeferLoaderTimeCameraReads { get; init; }
}
