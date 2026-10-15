using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>A validated <see cref="IRoomEnemyLoad"/>; required capabilities are non-null at construction.</summary>
public sealed record RoomEnemyLoadRequest : IRoomEnemyLoad
{
    /// <summary>Creates a load of the population at <paramref name="populationPointer"/> with <paramref name="services"/>.</summary>
    public RoomEnemyLoadRequest(ISnesAddressSpace bus, ushort populationPointer, ushort tilesetPointer,
        SnesVram vram, SnesCgram cgram, IRoomEnemyServices services)
    {
        Bus = Ensure.NotNull(bus);
        PopulationPointer = populationPointer;
        TilesetPointer = tilesetPointer;
        Vram = Ensure.NotNull(vram);
        Cgram = Ensure.NotNull(cgram);
        Services = Ensure.NotNull(services);
    }

    /// <inheritdoc />
    public ISnesAddressSpace Bus { get; }

    /// <inheritdoc />
    public ushort PopulationPointer { get; }

    /// <inheritdoc />
    public ushort TilesetPointer { get; }

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
    public ushort ControllerInput { get; init; }

    /// <inheritdoc />
    public Position16<RoomPixels> Camera { get; init; }

    /// <inheritdoc />
    public GunshipLoadScenario GunshipScenario { get; init; } = GunshipLoadScenario.Ordinary;

    /// <inheritdoc />
    public bool DeferLoaderTimeCameraReads { get; init; }
}
