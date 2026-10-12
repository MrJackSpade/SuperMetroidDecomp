using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Room enemy services a verification case drives directly (#633). Each service the case sets wins;
/// any other call falls through to the services <see cref="RoomEnemySystem.Load"/> installed, and fails
/// at the dependency when neither provides it.
/// </summary>
internal sealed class VerificationRoomEnemyServices(IRoomEnemyServices? loaded) : IRoomEnemyServices,
    IEnemyRandomNumbers, IGlobalEventFlags, IAreaBossFlags, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomPlmPresence
{
    /// <summary>Gets or sets next random.</summary>
    public Func<ushort>? NextRandom { get; set; }
    /// <summary>Gets or sets read random.</summary>
    public Func<ushort>? ReadRandom { get; set; }
    /// <summary>Gets or sets seed random.</summary>
    public Action<ushort>? SeedRandom { get; set; }
    /// <summary>Gets or sets has event.</summary>
    public Func<EventNumber, bool>? HasEvent { get; set; }
    /// <summary>Gets or sets is area boss defeated.</summary>
    public Func<bool>? IsAreaBossDefeated { get; set; }
    /// <summary>Gets or sets is area mini boss defeated.</summary>
    public Func<bool>? IsAreaMiniBossDefeated { get; set; }
    /// <summary>Gets or sets is area torizo defeated.</summary>
    public Func<bool>? IsAreaTorizoDefeated { get; set; }
    /// <summary>Gets or sets set area torizo defeated.</summary>
    public Action? SetAreaTorizoDefeated { get; set; }
    /// <summary>Gets or sets set room scroll state.</summary>
    public Action<int, RoomScrollState>? SetRoomScrollState { get; set; }
    /// <summary>Gets or sets set mother brain bg2 scroll.</summary>
    public Action<Position16<Bg2Pixels>>? SetMotherBrainBg2Scroll { get; set; }
    /// <summary>Gets or sets increment mother brain glass room argument.</summary>
    public Action? IncrementMotherBrainGlassRoomArgument { get; set; }

    /// <summary>Gets random.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Gets events.</summary>
    public IGlobalEventFlags Events => this;
    /// <summary>Gets bosses.</summary>
    public IAreaBossFlags Bosses => this;
    /// <summary>Gets plms.</summary>
    public IRoomPlmPresence Plms => this;
    /// <summary>Gets room scroll.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Gets samus controls.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Gets mother brain.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Generate.</summary>
    public ushort Generate() => NextRandom is { } next ? next() : Loaded("generate a random number").Random.Generate();

    /// <summary>Read.</summary>
    public ushort Read() => ReadRandom is { } read ? read() : Loaded("read the random-number word").Random.Read();

    /// <summary>Seed.</summary>
    public void Seed(ushort value)
    {
        if (SeedRandom is { } seed)
            seed(value);
        else
            Loaded("seed the random-number word").Random.Seed(value);
    }

    /// <summary>Has.</summary>
    public bool Has(EventNumber eventNumber) =>
        HasEvent is { } has ? has(eventNumber) : Loaded("read global event bits").Events.Has(eventNumber);

    /// <summary>Set.</summary>
    public void Set(EventNumber eventNumber) => Loaded("set a global event bit").Events.Set(eventNumber);

    /// <summary>Clear.</summary>
    public void Clear(EventNumber eventNumber) => Loaded("clear a global event bit").Events.Clear(eventNumber);

    /// <summary>Checks whether defeated.</summary>
    public bool IsDefeated(BossBits bits) => (bits switch
    {
        BossBits.AreaBoss => IsAreaBossDefeated,
        BossBits.AreaMiniBoss => IsAreaMiniBossDefeated,
        BossBits.AreaTorizo => IsAreaTorizoDefeated,
        _ => null,
    }) is { } query ? query() : Loaded($"read the area {bits} flag").Bosses.IsDefeated(bits);

    /// <summary>Sets defeated.</summary>
    public void SetDefeated(BossBits bits)
    {
        if (bits == BossBits.AreaTorizo && SetAreaTorizoDefeated is { } setTorizo)
            setTorizo();
        else
            Loaded($"set the area {bits} flag").Bosses.SetDefeated(bits);
    }

    /// <summary>Read.</summary>
    RoomScrollState IRoomScrollZoneAccess.Read(int index) => Loaded("read a room scroll state").RoomScroll.Read(index);

    /// <summary>Write.</summary>
    public void Write(int index, RoomScrollState state)
    {
        if (SetRoomScrollState is { } write)
            write(index, state);
        else
            Loaded("write a room scroll state").RoomScroll.Write(index, state);
    }

    /// <summary>Sets controls enabled.</summary>
    public void SetControlsEnabled(bool enabled) =>
        Loaded("change Samus control enablement").SamusControls.SetControlsEnabled(enabled);

    /// <summary>Increments glass room argument.</summary>
    public void IncrementGlassRoomArgument()
    {
        if (IncrementMotherBrainGlassRoomArgument is { } increment)
            increment();
        else
            Loaded("advance Mother Brain's glass-room argument").MotherBrain.IncrementGlassRoomArgument();
    }

    /// <summary>Sets layer blending default.</summary>
    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        Loaded("write Mother Brain's layer-blending configuration").MotherBrain.SetLayerBlendingDefault(configuration);

    /// <summary>Sets bg2 scroll.</summary>
    public void SetBg2Scroll(Position16<Bg2Pixels> scroll)
    {
        if (SetMotherBrainBg2Scroll is { } set)
            set(scroll);
        else
            Loaded("write Mother Brain's BG2 scroll registers").MotherBrain.SetBg2Scroll(scroll);
    }

    /// <summary>Checks whether active header.</summary>
    public bool HasActiveHeader(PlmHeaderId header) =>
        Loaded("query the active room PLM population").Plms.HasActiveHeader(header);

    /// <summary>Loaded.</summary>
    private IRoomEnemyServices Loaded(string operation) => loaded ?? throw new InvalidOperationException(
        $"The verification case did not install a service to {operation}, and the enemy system was not loaded with one.");
}

/// <summary>Verification access to the services of a <see cref="RoomEnemySystem"/> (#633).</summary>
internal static class RoomEnemyServicesAccess
{
    /// <summary>Provides extension members for <c>RoomEnemySystem</c>.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>The case's own services, installed over whatever <c>Load</c> installed on first use.</summary>
        internal VerificationRoomEnemyServices VerificationServices()
        {
            // The installed services, if any; the case layers its own over them.
            IRoomEnemyServices? current = PrivateState.Field<IRoomEnemyServices?>(self.Room, "<Services>k__BackingField");
            if (current is VerificationRoomEnemyServices installed)
                return installed;
            var services = new VerificationRoomEnemyServices(current);
            self.Room.Services = services;
            return services;
        }
    }
}
