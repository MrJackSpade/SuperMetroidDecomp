using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Room enemy services a verification case drives directly (#633). Each service the case sets wins;
/// any other call falls through to the services <see cref="RoomEnemySystem.Load"/> installed, and fails
/// at the dependency when neither provides it.
/// </summary>
internal sealed class VerificationRoomEnemyServices(IRoomEnemyServices? loaded) : IRoomEnemyServices,
    IEnemyRandomNumbers, IGlobalEventFlags, IZebetiteProgress, IAreaBossFlags, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomObjectPresence
{
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
    /// <summary>Optional case-owned query for the area's major boss flag.</summary>
    public Func<bool>? IsAreaBossDefeated { get; set; }
    /// <summary>Optional case-owned query for the area's mini-boss flag.</summary>
    public Func<bool>? IsAreaMiniBossDefeated { get; set; }
    /// <summary>Optional case-owned query for the area's Torizo flag.</summary>
    public Func<bool>? IsAreaTorizoDefeated { get; set; }
    /// <summary>Optional case-owned callback when the area's Torizo is defeated.</summary>
    public Action? SetAreaTorizoDefeated { get; set; }
    /// <summary>Optional case-owned callback for room scroll-zone writes.</summary>
    public Action<int, RoomScrollState>? SetRoomScrollState { get; set; }
    /// <summary>Optional case-owned callback for Mother Brain glass hit-count increments.</summary>
    public Action? IncrementMotherBrainGlassRoomArgument { get; set; }

    /// <summary>Exposes this fixture's RNG overrides through the enemy service contract.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Exposes this fixture's event query override through the enemy service contract.</summary>
    public IGlobalEventFlags Events => this;
    /// <summary>Exposes this fixture's Zebetite count override through the enemy service contract.</summary>
    public IZebetiteProgress ZebetiteProgress => this;
    /// <summary>Exposes this fixture's boss-flag overrides through the enemy service contract.</summary>
    public IAreaBossFlags Bosses => this;
    /// <summary>Uses loaded room services for active PLM queries.</summary>
    public IRoomObjectPresence RoomObjects => this;
    /// <summary>Exposes this fixture's room scroll-write override through the enemy service contract.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Uses loaded room services for Samus control changes.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Exposes this fixture's Mother Brain overrides through the enemy service contract.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Advances RNG through a case callback or the services installed by room loading.</summary>
    public int Generate() => NextRandom is { } next ? next() : Loaded("generate a random number").Random.Generate();

    /// <summary>Samples current RNG without advancing it through the override or loaded services.</summary>
    public int Read() => ReadRandom is { } read ? read() : Loaded("read the random-number word").Random.Read();

    /// <summary>Seeds RNG through a case callback or the services installed by room loading.</summary>
    public void Seed(int value)
    {
        if (SeedRandom is { } seed)
            seed(checked((ushort)value));
        else
            Loaded("seed the random-number word").Random.Seed(value);
    }

    /// <summary>Queries a global event through the fixture override or loaded services.</summary>
    public bool Has(StoryEvent eventNumber) =>
        HasEvent is { } has ? has(eventNumber) : Loaded("read global event bits").Events.Has(eventNumber);

    /// <summary>Forwards event writes to loaded room services.</summary>
    public void Set(StoryEvent eventNumber) => Loaded("set a global event bit").Events.Set(eventNumber);

    /// <summary>Reads the Zebetite count through the case override or loaded services; writes go to loaded services.</summary>
    int IZebetiteProgress.ZebetitesDestroyed
    {
        get => ReadZebetitesDestroyed is { } read
            ? read()
            : Loaded("read the Zebetites destroyed").ZebetiteProgress.ZebetitesDestroyed;
        set => Loaded("advance the Zebetites destroyed").ZebetiteProgress.ZebetitesDestroyed = value;
    }

    /// <summary>Queries the selected boss bit through its case callback or loaded services.</summary>
    public bool IsDefeated(DefeatedBosses bosses) => (bosses switch
    {
        DefeatedBosses.AreaBoss => IsAreaBossDefeated,
        DefeatedBosses.AreaMiniBoss => IsAreaMiniBossDefeated,
        DefeatedBosses.AreaTorizo => IsAreaTorizoDefeated,
        DefeatedBosses.None => null,
        _ => null,
    }) is { } query ? query() : Loaded($"read the area {bosses} flag").Bosses.IsDefeated(bosses);

    /// <summary>Uses the fixture's Torizo callback when supplied; otherwise forwards the boss flag write.</summary>
    public void SetDefeated(DefeatedBosses bosses)
    {
        if (bosses == DefeatedBosses.AreaTorizo && SetAreaTorizoDefeated is { } setTorizo)
            setTorizo();
        else
            Loaded($"set the area {bosses} flag").Bosses.SetDefeated(bosses);
    }

    /// <summary>Reads scroll-zone state from the services installed by room loading.</summary>
    RoomScrollState IRoomScrollZoneAccess.Read(int index) => Loaded("read a room scroll state").RoomScroll.Read(index);

    /// <summary>Writes scroll-zone state through the case callback or loaded services.</summary>
    public void Write(int index, RoomScrollState state)
    {
        if (SetRoomScrollState is { } write)
            write(index, state);
        else
            Loaded("write a room scroll state").RoomScroll.Write(index, state);
    }

    /// <summary>Writes a screen's scroll state through the services installed by room loading.</summary>
    public void Write(Position16<RoomScreens> screen, RoomScrollState state) =>
        Loaded("write a screen's scroll state").RoomScroll.Write(screen, state);

    /// <summary>Forwards enabling Samus's controls to loaded room services.</summary>
    public void EnableControls() => Loaded("enable Samus's controls").SamusControls.EnableControls();

    /// <inheritdoc />
    public void DisableControls() => Loaded("disable Samus's controls").SamusControls.DisableControls();

    /// <summary>Advances the Mother Brain glass hit count through an override or loaded services.</summary>
    public void IncrementGlassRoomArgument()
    {
        if (IncrementMotherBrainGlassRoomArgument is { } increment)
            increment();
        else
            Loaded("advance Mother Brain's glass-room argument").MotherBrain.IncrementGlassRoomArgument();
    }

    /// <summary>Shows a frame on the part of the loaded room's turning floor it animates.</summary>
    public void ShowFloorFrame(CeresFloorAnimationFrame frame) =>
        Loaded("animate the room's turning floor").ShowFloorFrame(frame);

    /// <summary>Whether the loaded room's Bomb Torizo statue hand still stands.</summary>
    public bool BombTorizoHandRemains => Loaded("ask whether the statue's hand still stands").RoomObjects.BombTorizoHandRemains;

    /// <summary>Fails at the missing dependency when neither the case nor room loading supplied a service.</summary>
    private IRoomEnemyServices Loaded(string operation) => loaded ?? throw new InvalidOperationException(
        $"The verification case did not install a service to {operation}, and the enemy system was not loaded with one.");
}
