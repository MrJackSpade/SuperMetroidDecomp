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
    public Func<ushort>? NextRandom { get; set; }
    public Func<ushort>? ReadRandom { get; set; }
    public Action<ushort>? SeedRandom { get; set; }
    public Func<EventNumber, bool>? HasEvent { get; set; }
    public Func<bool>? IsAreaBossDefeated { get; set; }
    public Func<bool>? IsAreaMiniBossDefeated { get; set; }
    public Func<bool>? IsAreaTorizoDefeated { get; set; }
    public Action? SetAreaTorizoDefeated { get; set; }
    public Action<int, RoomScrollState>? SetRoomScrollState { get; set; }
    public Action<Position16<Bg2Pixels>>? SetMotherBrainBg2Scroll { get; set; }
    public Action? IncrementMotherBrainGlassRoomArgument { get; set; }

    public IEnemyRandomNumbers Random => this;
    public IGlobalEventFlags Events => this;
    public IAreaBossFlags Bosses => this;
    public IRoomPlmPresence Plms => this;
    public IRoomScrollZoneAccess RoomScroll => this;
    public ISamusControlLock SamusControls => this;
    public IMotherBrainRoomEffects MotherBrain => this;

    public ushort Generate() => NextRandom is { } next ? next() : Loaded("generate a random number").Random.Generate();

    public ushort Read() => ReadRandom is { } read ? read() : Loaded("read the random-number word").Random.Read();

    public void Seed(ushort value)
    {
        if (SeedRandom is { } seed)
            seed(value);
        else
            Loaded("seed the random-number word").Random.Seed(value);
    }

    public bool Has(EventNumber eventNumber) =>
        HasEvent is { } has ? has(eventNumber) : Loaded("read global event bits").Events.Has(eventNumber);

    public void Set(EventNumber eventNumber) => Loaded("set a global event bit").Events.Set(eventNumber);

    public void Clear(EventNumber eventNumber) => Loaded("clear a global event bit").Events.Clear(eventNumber);

    public bool IsDefeated(BossBits bits) => (bits switch
    {
        BossBits.AreaBoss => IsAreaBossDefeated,
        BossBits.AreaMiniBoss => IsAreaMiniBossDefeated,
        BossBits.AreaTorizo => IsAreaTorizoDefeated,
        _ => null,
    }) is { } query ? query() : Loaded($"read the area {bits} flag").Bosses.IsDefeated(bits);

    public void SetDefeated(BossBits bits)
    {
        if (bits == BossBits.AreaTorizo && SetAreaTorizoDefeated is { } setTorizo)
            setTorizo();
        else
            Loaded($"set the area {bits} flag").Bosses.SetDefeated(bits);
    }

    RoomScrollState IRoomScrollZoneAccess.Read(int index) => Loaded("read a room scroll state").RoomScroll.Read(index);

    public void Write(int index, RoomScrollState state)
    {
        if (SetRoomScrollState is { } write)
            write(index, state);
        else
            Loaded("write a room scroll state").RoomScroll.Write(index, state);
    }

    public void SetControlsEnabled(bool enabled) =>
        Loaded("change Samus control enablement").SamusControls.SetControlsEnabled(enabled);

    public void IncrementGlassRoomArgument()
    {
        if (IncrementMotherBrainGlassRoomArgument is { } increment)
            increment();
        else
            Loaded("advance Mother Brain's glass-room argument").MotherBrain.IncrementGlassRoomArgument();
    }

    public void SetLayerBlendingDefault(LayerBlendingConfiguration configuration) =>
        Loaded("write Mother Brain's layer-blending configuration").MotherBrain.SetLayerBlendingDefault(configuration);

    public void SetBg2Scroll(Position16<Bg2Pixels> scroll)
    {
        if (SetMotherBrainBg2Scroll is { } set)
            set(scroll);
        else
            Loaded("write Mother Brain's BG2 scroll registers").MotherBrain.SetBg2Scroll(scroll);
    }

    public bool HasActiveHeader(PlmHeaderId header) =>
        Loaded("query the active room PLM population").Plms.HasActiveHeader(header);

    private IRoomEnemyServices Loaded(string operation) => loaded ?? throw new InvalidOperationException(
        $"The verification case did not install a service to {operation}, and the enemy system was not loaded with one.");
}

/// <summary>Verification access to the services of a <see cref="RoomEnemySystem"/> (#633).</summary>
internal static class RoomEnemyServicesAccess
{
    extension(RoomEnemySystem self)
    {
        /// <summary>The case's own services, installed over whatever <c>Load</c> installed on first use.</summary>
        internal VerificationRoomEnemyServices VerificationServices()
        {
            // The installed services, if any; the case layers its own over them.
            IRoomEnemyServices? current = self.Room.installedServices;
            if (current is VerificationRoomEnemyServices installed)
                return installed;
            var services = new VerificationRoomEnemyServices(current);
            self.Room.Services = services;
            return services;
        }
    }
}
