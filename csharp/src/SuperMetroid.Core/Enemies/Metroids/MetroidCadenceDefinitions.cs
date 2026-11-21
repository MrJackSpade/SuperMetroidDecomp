namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The counter cadences and phases of the Metroids' and Mochtroids' drains and hurt flash (#1606).</summary>
internal static class MetroidCadenceDefinitions
{
    /// <summary>
    /// The enemy passes on which a Metroid or Mochtroid draining Samus queues the energy-drain sound: the last place of
    /// each period of the enemy-pass counter, while Samus has at least the drain sound's least health. Every draining actor queues its
    /// own request.
    /// </summary>
    internal static readonly UpdateCadence DrainSound = new(8, 7);

    /// <summary>
    /// The phase of a hurt Metroid's flash timer on which its outer bodies show its own colours: for two updates of
    /// every four, while they show the hurt flash for the other two.
    /// </summary>
    internal static readonly UpdateAlternation HurtOuterBodiesShowOwnColours = new(2);
}
