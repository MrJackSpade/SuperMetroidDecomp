namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a lethal shot does to an enemy (#2560): one closed choice of the generic death, the generic death after a stage of
/// the species', a death of the species' that may replace the generic one, or a death the species owns outright.
/// </summary>
internal abstract partial record EnemyShotLethal
{
    /// <summary>The generic death animation.</summary>
    internal static EnemyShotLethal Generic { get; } = new GenericDeath();

    /// <summary>A death the species owns: the generic death does not run.</summary>
    internal static EnemyShotLethal SpeciesOwned { get; } = new SpeciesOwnedDeath();

    /// <summary>The generic death.</summary>
    private sealed partial record GenericDeath : EnemyShotLethal;

    /// <summary>The species' own death, which the generic death leaves alone.</summary>
    internal sealed partial record SpeciesOwnedDeath : EnemyShotLethal;

    /// <summary>A stage of the species' that runs immediately before the generic death.</summary>
    /// <param name="Stage">The stage.</param>
    internal sealed partial record AfterStage(Action<EnemyShotHitContext> Stage) : EnemyShotLethal;

    /// <summary>A death of the species' in place of the generic one; it reports whether it replaced it.</summary>
    /// <param name="Death">The death, which reports whether it replaced the generic death.</param>
    internal sealed partial record Replaced(Func<EnemyShotHitContext, bool> Death) : EnemyShotLethal;
}
