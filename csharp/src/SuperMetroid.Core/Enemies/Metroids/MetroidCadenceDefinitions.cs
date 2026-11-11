namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The counter cadences and phases of the Metroids' and Mochtroids' drains and hurt flash (#1606).</summary>
internal static class MetroidCadenceDefinitions
{
    /// <summary>
    /// The enemy passes on which a Metroid or Mochtroid draining Samus queues the energy-drain sound: every eighth pass
    /// of the enemy-pass counter, on its last place, while Samus has 30 energy or more. Every draining actor queues its
    /// own request.
    /// </summary>
    internal static readonly UpdateCadence DrainSoundEveryEighthEnemyPass = new(8, 7);

    /// <summary>
    /// The phase of a hurt Metroid's flash timer on which its outer bodies show its own colours: set for two updates of
    /// every four, while they show the hurt flash for the other two.
    /// </summary>
    internal static readonly UpdateCounterField HurtOuterBodiesShowOwnColours = new(1, 1);

    /// <summary>
    /// The phase of a shaking Mochtroid's shake timer that picks its displacement: the timer's place in a cycle of four
    /// steps, each held for two updates, indexing <see cref="MochtroidShakeDefinitions.Offsets"/>.
    /// </summary>
    internal static readonly UpdateCounterField MochtroidShakeStep = new(1, 2);
}
