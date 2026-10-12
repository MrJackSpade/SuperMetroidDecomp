using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The cartridge random-number word as enemy AI uses it (#633).</summary>
public interface IEnemyRandomNumbers
{
    /// <summary>Advances the RNG (<c>GenerateRandomNumber</c>) and returns the new word.</summary>
    ushort Generate();

    /// <summary>Samples the current random word without advancing it.</summary>
    ushort Read();

    /// <summary>Overwrites the random word, as some initializers seed it.</summary>
    void Seed(ushort value);
}

/// <summary>Global cartridge event bits.</summary>
public interface IGlobalEventFlags
{
    /// <summary>Whether <paramref name="eventNumber"/> is set.</summary>
    bool Has(EventNumber eventNumber);

    /// <summary>Sets <paramref name="eventNumber"/>.</summary>
    void Set(EventNumber eventNumber);

    /// <summary>Clears <paramref name="eventNumber"/>.</summary>
    void Clear(EventNumber eventNumber);
}

/// <summary>Reads the current area's boss, mini-boss and Torizo defeat bits (#633).</summary>
public interface IAreaBossFlagReader
{
    /// <summary>Whether any of <paramref name="bits"/> is set for the current area.</summary>
    bool IsDefeated(BossBits bits);
}

/// <summary>The current area's boss, mini-boss and Torizo defeat bits.</summary>
public interface IAreaBossFlags : IAreaBossFlagReader
{
    /// <summary>Sets <paramref name="bits"/> for the current area.</summary>
    void SetDefeated(BossBits bits);
}

/// <summary>The current room's scroll-zone states.</summary>
public interface IRoomScrollZoneAccess
{
    /// <summary>The state of scroll zone <paramref name="index"/>.</summary>
    RoomScrollState Read(int index);

    /// <summary>Stores <paramref name="state"/> for scroll zone <paramref name="index"/>.</summary>
    void Write(int index, RoomScrollState state);
}

/// <summary>The stationary-script lock on Samus's controls.</summary>
public interface ISamusControlLock
{
    /// <summary>Enables or disables Samus's controls.</summary>
    void SetControlsEnabled(bool enabled);
}

/// <summary>The room-level effects Mother Brain's fight drives.</summary>
public interface IMotherBrainRoomEffects
{
    /// <summary>Advances the glass room's PLM argument.</summary>
    void IncrementGlassRoomArgument();

    /// <summary>Sets the default layer-blending configuration.</summary>
    void SetLayerBlendingDefault(LayerBlendingConfiguration configuration);

    /// <summary>Writes the BG2 scroll registers.</summary>
    void SetBg2Scroll(Position16<Bg2Pixels> scroll);
}

/// <summary>Queries the active room PLM population.</summary>
public interface IRoomPlmPresence
{
    /// <summary>Whether a PLM with <paramref name="header"/> is active.</summary>
    bool HasActiveHeader(PlmHeaderId header);
}

/// <summary>Room scroll access shared by every enemy service set that changes the camera's zones.</summary>
public interface IRoomScrollServices
{
    /// <summary>The room's scroll zones.</summary>
    IRoomScrollZoneAccess RoomScroll { get; }
}

/// <summary>What the Chozo statues' room setup needs: Samus's control lock and the scroll zones.</summary>
public interface IChozoStatueRoomServices : IRoomScrollServices
{
    /// <summary>Samus's control lock.</summary>
    ISamusControlLock SamusControls { get; }
}

/// <summary>What Mother Brain's fight needs: the room effects and the scroll zones.</summary>
public interface IMotherBrainRoomServices : IRoomScrollServices
{
    /// <summary>Mother Brain's room effects.</summary>
    IMotherBrainRoomEffects MotherBrain { get; }
}

/// <summary>
/// The cartridge-state services enemy AI calls during a room: the union of every child operation's
/// service contract plus the RNG, events, boss bits and PLM presence the AI uses directly.
/// </summary>
public interface IRoomEnemyServices : IChozoStatueRoomServices, IMotherBrainRoomServices
{
    /// <summary>The cartridge RNG.</summary>
    IEnemyRandomNumbers Random { get; }

    /// <summary>Global event bits.</summary>
    IGlobalEventFlags Events { get; }

    /// <summary>The current area's boss bits.</summary>
    IAreaBossFlags Bosses { get; }

    /// <summary>The active PLM population.</summary>
    IRoomPlmPresence Plms { get; }
}

/// <summary>Where a room's enemy population and its graphics come from.</summary>
public interface IRoomEnemyPopulationSource
{
    /// <summary>The bus the population, definitions and tile graphics are read through.</summary>
    ISnesAddressSpace Bus { get; }

    /// <summary>The room state's enemy population pointer.</summary>
    ushort PopulationPointer { get; }

    /// <summary>The room state's enemy graphics-set pointer.</summary>
    ushort TilesetPointer { get; }

    /// <summary>VRAM the enemy graphics load into.</summary>
    SnesVram Vram { get; }

    /// <summary>CGRAM the enemy palettes load into.</summary>
    SnesCgram Cgram { get; }
}

/// <summary>
/// The actors and input enemy initialization and AI read: room terrain, Samus, the held controller
/// mask and the camera. Terrain and Samus are absent only in fixtures without them; an AI routine that
/// needs one fails where it reads it.
/// </summary>
public interface IRoomEnemyActorContext
{
    /// <summary>Room terrain and collision data, if the room has been built.</summary>
    RoomLevelData? Level { get; }

    /// <summary>Samus, if present.</summary>
    SamusState? Samus { get; }

    /// <summary>The raw held SNES controller mask.</summary>
    ushort ControllerInput { get; }

    /// <summary>The camera's room position.</summary>
    Position16<RoomPixels> Camera { get; }
}

/// <summary>
/// Everything <see cref="RoomEnemySystem.Load"/> needs: the population source and actor context its
/// child loaders read, the services the AI keeps for the room, and the load scenario it owns directly.
/// </summary>
public interface IRoomEnemyLoad : IRoomEnemyPopulationSource, IRoomEnemyActorContext
{
    /// <summary>The cartridge-state services the room's enemy AI calls.</summary>
    IRoomEnemyServices Services { get; }

    /// <summary>Which gunship arrival this load performs.</summary>
    GunshipLoadScenario GunshipScenario { get; }

    /// <summary>Whether camera reads made at load time are deferred because the room streams in through a door.</summary>
    bool DeferLoaderTimeCameraReads { get; }
}

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
