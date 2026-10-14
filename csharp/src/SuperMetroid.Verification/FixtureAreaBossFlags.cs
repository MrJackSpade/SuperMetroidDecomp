using SuperMetroid.Core.Progression;

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
