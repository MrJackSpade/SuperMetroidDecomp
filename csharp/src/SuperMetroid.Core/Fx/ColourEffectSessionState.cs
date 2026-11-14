using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The colour-effect state room loading never clears: whether the effects run, and the heat phases. Each room's
/// <see cref="ColourEffectSystem"/> reads and writes them here.
/// </summary>
public sealed class ColourEffectSessionState
{
    /// <summary>
    /// Whether the colour effects run, checked first each update. Loading a game turns them on; the escape timer's
    /// expiry turns them off.
    /// </summary>
    public bool ColourEffectsRun { get; private set; } = true;

    /// <summary>
    /// The heat phase the heated room's glow published last, which Samus's heat shimmer, taking its turn before the glow,
    /// follows on the next update.
    /// </summary>
    internal HeatPhase PublishedHeatPhase { get; set; } = HeatPhase.First;

    /// <summary>The heat phase Samus's heat shimmer last restarted on; it restarts only when the published phase differs.</summary>
    internal HeatPhase ShimmerHeatPhase { get; set; } = HeatPhase.First;

    /// <summary>Stops the colour effects from running.</summary>
    public void StopColourEffects() => ColourEffectsRun = false;
}
