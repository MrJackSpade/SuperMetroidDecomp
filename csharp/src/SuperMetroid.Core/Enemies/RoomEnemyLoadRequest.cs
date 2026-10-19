using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>A validated <see cref="IRoomEnemyLoad"/>; required capabilities are non-null at construction.</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
public sealed record RoomEnemyLoadRequest : IRoomEnemyLoad
{
    /// <summary>Creates a load of <paramref name="population"/>.</summary>
    public RoomEnemyLoadRequest(RoomEnemyPopulationDefinition population,
        RoomEnemyGraphicsSetDefinition graphicsSet, SnesVram vram, SnesCgram cgram)
    {
        Population = Ensure.NotNull(population);
        GraphicsSet = Ensure.NotNull(graphicsSet);
        Vram = Ensure.NotNull(vram);
        Cgram = Ensure.NotNull(cgram);
    }

    /// <inheritdoc />
    public RoomEnemyPopulationDefinition Population { get; }

    /// <inheritdoc />
    public RoomEnemyGraphicsSetDefinition GraphicsSet { get; }

    /// <inheritdoc />
    public SnesVram Vram { get; }

    /// <inheritdoc />
    public SnesCgram Cgram { get; }

    /// <inheritdoc />
    public RoomLevelData? Level { get; init; }

    /// <inheritdoc />
    public SamusState? Samus { get; init; }

    /// <inheritdoc />
    public SnesButton ControllerInput { get; init; }

    /// <inheritdoc />
    public Position16<RoomPixels> Camera { get; init; }

    /// <inheritdoc />
    public GunshipLoadScenario GunshipScenario { get; init; } = GunshipLoadScenario.Ordinary;

    /// <inheritdoc />
    public bool DeferLoaderTimeCameraReads { get; init; }
}
