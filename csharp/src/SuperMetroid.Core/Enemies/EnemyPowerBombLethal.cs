namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a lethal Power Bomb blast does to an enemy (#2560): one closed choice of the generic death, the generic death after
/// a stage of the species', a death of the species' in its place, or a death the species owns outright.
/// </summary>
internal abstract partial record EnemyPowerBombLethal
{
    /// <summary>The generic death animation.</summary>
    internal static EnemyPowerBombLethal Generic { get; } = new GenericDeath();

    /// <summary>A death the species owns: the generic death does not run.</summary>
    internal static EnemyPowerBombLethal SpeciesOwned { get; } = new SpeciesOwnedDeath();

    /// <summary>The generic death.</summary>
    private sealed partial record GenericDeath : EnemyPowerBombLethal;

    /// <summary>The species' own death, which the generic death leaves alone.</summary>
    internal sealed partial record SpeciesOwnedDeath : EnemyPowerBombLethal;

    /// <summary>A stage of the species' that runs immediately before the generic death.</summary>
    /// <param name="Stage">The stage.</param>
    internal sealed partial record AfterStage(EnemyPowerBombStage Stage) : EnemyPowerBombLethal;

    /// <summary>A death of the species' in place of the generic one.</summary>
    /// <param name="Death">The death.</param>
    internal sealed partial record Replaced(EnemyPowerBombStage Death) : EnemyPowerBombLethal;
}
