using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Room enemy services a verification case drives directly (#633). Each service the case sets wins;
/// any other call falls through to the services <see cref="RoomEnemySystem.Load"/> installed. Without them, the random
/// numbers, the story events, the Zebetite count and the area's defeated bosses are those the room load installs for a new
/// game's room (the session's shared random numbers and <see cref="RoomAreaProgress"/>: nothing has happened yet), and any
/// other call fails at the dependency.
/// </summary>
/// <param name="loaded">The services the room load installed, or null for an enemy system no room load built.</param>
/// <param name="area">The area whose defeated bosses a new game's progress keeps for the room, when no room load did.</param>
internal sealed class VerificationRoomEnemyServices(IRoomEnemyServices? loaded, AreaId area = AreaId.Crateria) : IRoomEnemyServices,
    IEnemyRandomNumbers, IStoryEvents, IZebetiteProgress, IDefeatedBossesRecorder, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomObjectPresence
{
    /// <summary>
    /// A new game's shared state, whose random numbers a room load hands the room's enemies (#2554), for an enemy system no
    /// room load built.
    /// </summary>
    private readonly SharedGameState _newGame = new(new SharedRandom());

    /// <summary>
    /// A new game's progress in the room's area, which a room load installs as the room's progress (#2554), for an enemy
    /// system no room load built.
    /// </summary>
    private RoomAreaProgress NewGameProgress => field ??= new(_newGame, area);

    /// <summary>Optional case-owned callback for advancing the enemy RNG.</summary>
    public Func<ushort>? NextRandom { get; set; }
    /// <summary>Optional case-owned callback for sampling the current RNG word.</summary>
    public Func<ushort>? ReadRandom { get; set; }
    /// <summary>Optional case-owned callback for resetting the RNG word.</summary>
    public Action<ushort>? SeedRandom { get; set; }
    /// <summary>Optional case-owned query for global event flags.</summary>
    public Func<StoryEvent, bool>? HasEvent { get; set; }
    /// <summary>Optional case-owned query for how many Zebetites have been destroyed.</summary>
    public Func<int>? ReadZebetitesDestroyed { get; set; }
    /// <summary>Optional case-owned record of the Zebetites destroyed, which a Zebetite's death advances.</summary>
    public Action<int>? WriteZebetitesDestroyed { get; set; }
    /// <summary>Optional case-owned query for the area's major boss flag.</summary>
    public Func<bool>? IsAreaBossDefeated { get; set; }
    /// <summary>Optional case-owned query for the area's mini-boss flag.</summary>
    public Func<bool>? IsAreaMiniBossDefeated { get; set; }
    /// <summary>Optional case-owned query for the area's Torizo flag.</summary>
    public Func<bool>? IsAreaTorizoDefeated { get; set; }
    /// <summary>Optional case-owned callback when the area's Torizo is defeated.</summary>
    public Action? SetAreaTorizoDefeated { get; set; }
    /// <summary>Optional case-owned callback when the area's mini-boss is defeated.</summary>
    public Action? SetAreaMiniBossDefeated { get; set; }
    /// <summary>Optional case-owned callback for writes of a screen's scroll state.</summary>
    public Action<Position16<RoomScreens>, RoomScrollState>? SetRoomScrollState { get; set; }
    /// <summary>Optional case-owned callback for Mother Brain glass hit-count increments.</summary>
    public Action? HeadShotOnGlassCounted { get; set; }

    /// <summary>
    /// Mother Brain's glass as the case's room objects set it up before her body spawns (#2554), in place of the loaded
    /// services' room objects.
    /// </summary>
    public MotherBrainGlassObjects? RoomGlass { get; set; }

    /// <summary>Takes Samus's controls away (true) or gives them back (false), in place of the loaded services' lock.</summary>
    public Action<bool>? SetControlsLocked { get; set; }

    /// <summary>Exposes this fixture's RNG overrides through the enemy service contract.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Exposes this fixture's event query override through the enemy service contract.</summary>
    public IStoryEvents Events => this;
    /// <summary>Exposes this fixture's Zebetite count override through the enemy service contract.</summary>
    public IZebetiteProgress ZebetiteProgress => this;
    /// <summary>Exposes this fixture's boss-flag overrides through the enemy service contract.</summary>
    public IDefeatedBossesRecorder Bosses => this;
    /// <summary>Uses loaded room services for active PLM queries.</summary>
    public IRoomObjectPresence RoomObjects => this;
    /// <summary>Exposes this fixture's room scroll-write override through the enemy service contract.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Uses loaded room services for Samus control changes.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Exposes this fixture's Mother Brain overrides through the enemy service contract.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Advances RNG through a case callback, the services installed by room loading or a new game's.</summary>
    public int Generate() => NextRandom is { } next ? next() : loaded is { } services ? services.Random.Generate() : _newGame.NextRandom();

    /// <summary>Advances the fixture's random numbers once, as <see cref="Generate"/> does, its value unread.</summary>
    public void Advance() => _ = Generate();

    /// <summary>Samples current RNG without advancing it through the override, the loaded services or a new game's.</summary>
    public int Read() => ReadRandom is { } read ? read() : loaded is { } services ? services.Random.Read() : _newGame.RandomNumber;

    /// <summary>Reseeds RNG with a species' seed through a case callback, the services installed by room loading or a new game's.</summary>
    public void Reseed(SpeciesRandomReseed reseed)
    {
        if (SeedRandom is { } seed)
            seed(checked((ushort)Ensure.NotNull(reseed).Seed.Value));
        else if (loaded is { } services)
            services.Random.Reseed(reseed);
        else
            _newGame.Random.Reseed(reseed);
    }

    /// <summary>Queries a global event through the fixture override or loaded services.</summary>
    public bool Has(StoryEvent storyEvent) => HasEvent is { } has ? has(storyEvent) : Progress.Events.Has(storyEvent);

    /// <summary>Forwards event writes to the room's progress.</summary>
    public void Set(StoryEvent storyEvent) => Progress.Events.Set(storyEvent);

    /// <summary>Reads and writes the Zebetite count through the case overrides or loaded services.</summary>
    int IZebetiteProgress.ZebetitesDestroyed
    {
        get => ReadZebetitesDestroyed is { } read ? read() : Progress.ZebetiteProgress.ZebetitesDestroyed;
        set
        {
            if (WriteZebetitesDestroyed is { } write)
                write(value);
            else
                Progress.ZebetiteProgress.ZebetitesDestroyed = value;
        }
    }

    /// <summary>Queries the selected boss bit through its case callback or loaded services.</summary>
    public bool IsDefeated(DefeatedBosses bosses) => (bosses switch
    {
        DefeatedBosses.AreaBoss => IsAreaBossDefeated,
        DefeatedBosses.AreaMiniBoss => IsAreaMiniBossDefeated,
        DefeatedBosses.AreaTorizo => IsAreaTorizoDefeated,
        DefeatedBosses.None => null,
        _ => null,
    }) is { } query ? query() : Progress.Bosses.IsDefeated(bosses);

    /// <summary>Uses the fixture's Torizo or mini-boss callback when supplied; otherwise forwards the boss flag write.</summary>
    public void SetDefeated(DefeatedBosses bosses)
    {
        if (bosses == DefeatedBosses.AreaTorizo && SetAreaTorizoDefeated is { } setTorizo)
            setTorizo();
        else if (bosses == DefeatedBosses.AreaMiniBoss && SetAreaMiniBossDefeated is { } setMiniBoss)
            setMiniBoss();
        else
            Progress.Bosses.SetDefeated(bosses);
    }

    /// <summary>Writes a screen's scroll state through the case callback or the services installed by room loading.</summary>
    public void Write(Position16<RoomScreens> screen, RoomScrollState state)
    {
        if (SetRoomScrollState is { } write)
            write(screen, state);
        else
            Loaded("write a screen's scroll state").RoomScroll.Write(screen, state);
    }

    /// <summary>Forwards enabling Samus's controls to loaded room services.</summary>
    public void EnableControls()
    {
        if (SetControlsLocked is { } set)
            set(false);
        else
            Loaded("enable Samus's controls").SamusControls.EnableControls();
    }

    /// <inheritdoc />
    public void DisableControls()
    {
        if (SetControlsLocked is { } set)
            set(true);
        else
            Loaded("disable Samus's controls").SamusControls.DisableControls();
    }

    /// <summary>Advances the Mother Brain glass hit count through an override or loaded services.</summary>
    public void CountHeadShotOnGlass()
    {
        if (HeadShotOnGlassCounted is { } increment)
            increment();
        else if (RoomGlass is { } glass)
            glass.CountHeadShotOnGlass();
        else
            Loaded("count a head shot on Mother Brain's glass").MotherBrain.CountHeadShotOnGlass();
    }

    /// <summary>Confirms Mother Brain's glass loaded through the case's room objects or the loaded services.</summary>
    public void ConfirmGlassLoaded()
    {
        if (RoomGlass is { } glass)
            glass.ConfirmGlassLoaded();
        else
            Loaded("confirm Mother Brain's glass loaded").MotherBrain.ConfirmGlassLoaded();
    }

    /// <summary>Whether the loaded room's Bomb Torizo statue hand still stands.</summary>
    public bool BombTorizoHandRemains => Loaded("ask whether the statue's hand still stands").RoomObjects.BombTorizoHandRemains;

    /// <summary>The story events, Zebetite count and defeated bosses: the loaded room's, or a new game's in the room's area.</summary>
    private (IStoryEvents Events, IZebetiteProgress ZebetiteProgress, IDefeatedBossesRecorder Bosses) Progress =>
        loaded is { } services ? (services.Events, services.ZebetiteProgress, services.Bosses) : (NewGameProgress, NewGameProgress, NewGameProgress);

    /// <summary>Fails at the missing dependency when neither the case nor room loading supplied a service.</summary>
    private IRoomEnemyServices Loaded(string operation) => loaded ?? throw new InvalidOperationException(
        $"The verification case did not install a service to {operation}, and the enemy system was not loaded with one.");
}
