using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Event bits a focused enemy fixture installs (#633). An operation the fixture did not install fails
/// at the dependency, as a host without the service would.
/// </summary>
internal sealed class FixtureEventFlags : IGlobalEventFlags
{
    /// <summary>Answers event queries.</summary>
    public Func<EventNumber, bool>? HasEvent { get; init; }

    /// <summary>Receives event sets.</summary>
    public Action<EventNumber>? SetEvent { get; init; }

    /// <summary>Receives event clears.</summary>
    public Action<EventNumber>? ClearEvent { get; init; }

    /// <summary>Delegates an event query or fails at the missing fixture dependency.</summary>
    public bool Has(EventNumber eventNumber) => (HasEvent ?? throw Missing("read"))(eventNumber);

    /// <summary>Delegates an event set or fails at the missing fixture dependency.</summary>
    public void Set(EventNumber eventNumber) => (SetEvent ?? throw Missing("set"))(eventNumber);

    /// <summary>Delegates an event clear or fails at the missing fixture dependency.</summary>
    public void Clear(EventNumber eventNumber) => (ClearEvent ?? throw Missing("clear"))(eventNumber);

    /// <summary>Reports which event operation the fixture did not install.</summary>
    private static InvalidOperationException Missing(string operation) =>
        new($"The enemy fixture did not install an event {operation}.");
}

/// <summary>Area boss bits a focused enemy fixture installs (#633), per <see cref="BossBits"/> kind.</summary>
internal sealed class FixtureAreaBossFlags : IAreaBossFlags
{
    /// <summary>Answers the area-boss query.</summary>
    public Func<bool>? IsBossDefeated { get; init; }

    /// <summary>Receives the area-boss set.</summary>
    public Action? SetBossDefeated { get; init; }

    /// <summary>Answers the area-miniboss query.</summary>
    public Func<bool>? IsMiniBossDefeated { get; init; }

    /// <summary>Receives the area-miniboss set.</summary>
    public Action? SetMiniBossDefeated { get; init; }

    /// <summary>Answers the area-Torizo query.</summary>
    public Func<bool>? IsTorizoDefeated { get; init; }

    /// <summary>Receives the area-Torizo set.</summary>
    public Action? SetTorizoDefeated { get; init; }

    /// <summary>Selects exactly one installed boss-kind query and rejects unsupported bit combinations.</summary>
    public bool IsDefeated(BossBits bits) => (bits switch
    {
        BossBits.AreaBoss => IsBossDefeated,
        BossBits.AreaMiniBoss => IsMiniBossDefeated,
        BossBits.AreaTorizo => IsTorizoDefeated,
        _ => throw new ArgumentOutOfRangeException(nameof(bits), bits, "Enemy AI queries one boss kind at a time."),
    } ?? throw new InvalidOperationException($"The enemy fixture did not install a {bits} query."))();

    /// <summary>Selects exactly one installed boss-kind setter and rejects unsupported bit combinations.</summary>
    public void SetDefeated(BossBits bits) => (bits switch
    {
        BossBits.AreaBoss => SetBossDefeated,
        BossBits.AreaMiniBoss => SetMiniBossDefeated,
        BossBits.AreaTorizo => SetTorizoDefeated,
        _ => throw new ArgumentOutOfRangeException(nameof(bits), bits, "Enemy AI sets one boss kind at a time."),
    } ?? throw new InvalidOperationException($"The enemy fixture did not install a {bits} set."))();
}

/// <summary>Room scroll-zone writes a focused enemy fixture records (#633).</summary>
internal sealed class FixtureRoomScroll(Action<int, RoomScrollState> write) : IRoomScrollZoneAccess
{
    /// <summary>Fails because focused enemy fixtures install write observation only.</summary>
    public RoomScrollState Read(int index) =>
        throw new InvalidOperationException("The enemy fixture did not install room scroll reads.");

    /// <summary>Forwards a scroll-zone mutation to the fixture's recorder.</summary>
    public void Write(int index, RoomScrollState state) => write(index, state);
}
