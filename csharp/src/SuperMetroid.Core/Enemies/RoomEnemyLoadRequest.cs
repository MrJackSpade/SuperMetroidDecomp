using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>A validated <see cref="IRoomEnemyLoad"/>; required capabilities are non-null at construction.</summary>
public sealed record RoomEnemyLoadRequest : IRoomEnemyLoad
{
    /// <summary>Creates a load of <paramref name="population"/> with <paramref name="services"/>.</summary>
    public RoomEnemyLoadRequest(IRoomEnemyDefinitionSource definitions, RoomEnemyPopulationDefinition population,
        RoomEnemyGraphicsSetDefinition graphicsSet, SnesVram vram, SnesCgram cgram, IRoomEnemyServices services)
    {
        Definitions = Ensure.NotNull(definitions);
        Population = Ensure.NotNull(population);
        GraphicsSet = Ensure.NotNull(graphicsSet);
        Vram = Ensure.NotNull(vram);
        Cgram = Ensure.NotNull(cgram);
        Services = Ensure.NotNull(services);
    }

    /// <inheritdoc />
    public IRoomEnemyDefinitionSource Definitions { get; }

    /// <inheritdoc />
    public RoomEnemyPopulationDefinition Population { get; }

    /// <inheritdoc />
    public RoomEnemyGraphicsSetDefinition GraphicsSet { get; }

    /// <inheritdoc />
    public SnesVram Vram { get; }

    /// <inheritdoc />
    public SnesCgram Cgram { get; }

    /// <inheritdoc />
    public IRoomEnemyServices Services { get; }

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
