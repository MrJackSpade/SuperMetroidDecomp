using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Room enemy services a verification case drives directly (#633). Each service the case sets wins;
/// any other call falls through to the services <see cref="RoomEnemySystem.Load"/> installed, and fails
/// at the dependency when neither provides it.
/// </summary>
internal sealed class VerificationRoomEnemyServices(IRoomEnemyServices? loaded) : IRoomEnemyServices,
    IEnemyRandomNumbers, IGlobalEventFlags, IAreaBossFlags, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomPlmPresence
{
    /// <summary>Optional case-owned callback for advancing the enemy RNG.</summary>
    public Func<ushort>? NextRandom { get; set; }
    /// <summary>Optional case-owned callback for sampling the current RNG word.</summary>
    public Func<ushort>? ReadRandom { get; set; }
    /// <summary>Optional case-owned callback for resetting the RNG word.</summary>
    public Action<ushort>? SeedRandom { get; set; }
    /// <summary>Optional case-owned query for global event flags.</summary>
    public Func<EventNumber, bool>? HasEvent { get; set; }
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
    /// <summary>Optional case-owned callback for Mother Brain's BG2 scroll updates.</summary>
    public Action<Position16<Bg2Pixels>>? SetMotherBrainBg2Scroll { get; set; }
    /// <summary>Optional case-owned callback for Mother Brain glass hit-count increments.</summary>
    public Action? IncrementMotherBrainGlassRoomArgument { get; set; }

    /// <summary>Exposes this fixture's RNG overrides through the enemy service contract.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Exposes this fixture's event query override through the enemy service contract.</summary>
    public IGlobalEventFlags Events => this;
    /// <summary>Exposes this fixture's boss-flag overrides through the enemy service contract.</summary>
    public IAreaBossFlags Bosses => this;
    /// <summary>Uses loaded room services for active PLM queries.</summary>
    public IRoomPlmPresence Plms => this;
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
    public bool Has(EventNumber eventNumber) =>
        HasEvent is { } has ? has(eventNumber) : Loaded("read global event bits").Events.Has(eventNumber);

    /// <summary>Forwards event writes to loaded room services.</summary>
    public void Set(EventNumber eventNumber) => Loaded("set a global event bit").Events.Set(eventNumber);

    /// <summary>Forwards event clearing to loaded room services.</summary>
    public void Clear(EventNumber eventNumber) => Loaded("clear a global event bit").Events.Clear(eventNumber);

    /// <summary>Queries the selected boss bit through its case callback or loaded services.</summary>
    public bool IsDefeated(BossBits bits) => (bits switch
    {
        BossBits.AreaBoss => IsAreaBossDefeated,
        BossBits.AreaMiniBoss => IsAreaMiniBossDefeated,
        BossBits.AreaTorizo => IsAreaTorizoDefeated,
        _ => null,
    }) is { } query ? query() : Loaded($"read the area {bits} flag").Bosses.IsDefeated(bits);

    /// <summary>Uses the fixture's Torizo callback when supplied; otherwise forwards the boss flag write.</summary>
    public void SetDefeated(BossBits bits)
    {
        if (bits == BossBits.AreaTorizo && SetAreaTorizoDefeated is { } setTorizo)
            setTorizo();
        else
            Loaded($"set the area {bits} flag").Bosses.SetDefeated(bits);
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

    /// <summary>Forwards Samus control changes to loaded room services.</summary>
    public void SetControlsEnabled(bool enabled) =>
        Loaded("change Samus control enablement").SamusControls.SetControlsEnabled(enabled);

    /// <summary>Advances the Mother Brain glass hit count through an override or loaded services.</summary>
    public void IncrementGlassRoomArgument()
    {
        if (IncrementMotherBrainGlassRoomArgument is { } increment)
            increment();
        else
            Loaded("advance Mother Brain's glass-room argument").MotherBrain.IncrementGlassRoomArgument();
    }

    /// <summary>Forwards Mother Brain's default layer blend to loaded room services.</summary>
    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        Loaded("write Mother Brain's layer-blending configuration").MotherBrain.SetLayerBlendingDefault(configuration);

    /// <summary>Writes Mother Brain's BG2 scroll through an override or loaded services.</summary>
    public void SetBg2Scroll(Position16<Bg2Pixels> scroll)
    {
        if (SetMotherBrainBg2Scroll is { } set)
            set(scroll);
        else
            Loaded("write Mother Brain's BG2 scroll registers").MotherBrain.SetBg2Scroll(scroll);
    }

    /// <summary>Queries the loaded room's active PLM headers.</summary>
    public bool HasActiveHeader(PlmHeaderId header) =>
        Loaded("query the active room PLM population").Plms.HasActiveHeader(header);

    /// <summary>Fails at the missing dependency when neither the case nor room loading supplied a service.</summary>
    private IRoomEnemyServices Loaded(string operation) => loaded ?? throw new InvalidOperationException(
        $"The verification case did not install a service to {operation}, and the enemy system was not loaded with one.");
}
