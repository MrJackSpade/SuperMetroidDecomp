using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>Area boss bosses a focused enemy fixture installs (#633), per <see cref="DefeatedBosses"/> kind.</summary>
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
    public bool IsDefeated(DefeatedBosses bosses) => (bosses switch
    {
        DefeatedBosses.AreaBoss => IsBossDefeated,
        DefeatedBosses.AreaMiniBoss => IsMiniBossDefeated,
        DefeatedBosses.AreaTorizo => IsTorizoDefeated,
        DefeatedBosses.None => throw new ArgumentOutOfRangeException(nameof(bosses), bosses, "Enemy AI queries one boss kind at a time."),
        _ => throw new ArgumentOutOfRangeException(nameof(bosses), bosses, "Enemy AI queries one boss kind at a time."),
    } ?? throw new InvalidOperationException($"The enemy fixture did not install a {bosses} query."))();

    /// <summary>Selects exactly one installed boss-kind setter and rejects unsupported bit combinations.</summary>
    public void SetDefeated(DefeatedBosses bosses) => (bosses switch
    {
        DefeatedBosses.AreaBoss => SetBossDefeated,
        DefeatedBosses.AreaMiniBoss => SetMiniBossDefeated,
        DefeatedBosses.AreaTorizo => SetTorizoDefeated,
        DefeatedBosses.None => throw new ArgumentOutOfRangeException(nameof(bosses), bosses, "Enemy AI sets one boss kind at a time."),
        _ => throw new ArgumentOutOfRangeException(nameof(bosses), bosses, "Enemy AI sets one boss kind at a time."),
    } ?? throw new InvalidOperationException($"The enemy fixture did not install a {bosses} set."))();
}
