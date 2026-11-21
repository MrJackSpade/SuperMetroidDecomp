namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// What a running colour effect watches each update before its countdown runs (#1554, #1982). The effect's definition
/// names the condition (<see cref="ColourEffectDefinition"/>); the effect watches it from the update after it shows its
/// first frame until it ends.
/// </summary>
public abstract partial record ColourEffectCondition
{
    /// <summary>Closes the hierarchy to the conditions declared here.</summary>
    private ColourEffectCondition()
    {
    }

    /// <summary>
    /// The effect watches nothing (#2544): the condition of an effect whose definition names none, and of every effect before
    /// it shows its first frame.
    /// </summary>
    public sealed partial record WatchesNothing : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static WatchesNothing Instance { get; } = new();
    }

    /// <summary>
    /// While Samus stands higher in the room than <see cref="Height"/>, the effect restarts its cycle, the phase its
    /// animation loops from, on every update, so it shows the cycle's first frame each update instead of its flashes: the
    /// Crateria lightning stays calm while Samus is high on the surface.
    /// </summary>
    /// <param name="Height">The room height, in whole pixels, Samus must be above.</param>
    public sealed partial record RestartsWhileSamusIsHigh(int Height) : ColourEffectCondition;

    /// <summary>
    /// The effect ends on the first update the Torizo whose belly it lights (<see cref="ILitTorizo"/>), the Torizo whose
    /// fight start started it, has no health left.
    /// </summary>
    public sealed partial record EndsWhenTorizoDies : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static EndsWhenTorizoDies Instance { get; } = new();
    }

    /// <summary>The effect ends on the first update the area's mini-boss is recorded dead: Spore Spawn's spores.</summary>
    public sealed partial record EndsWhenAreaMiniBossDies : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static EndsWhenAreaMiniBossDies Instance { get; } = new();
    }

    /// <summary>
    /// Samus's heat shimmer: each update the heat burns a Samus without a heat-protecting suit, and the shimmer restarts
    /// on the heat phase the Norfair glow published when it changed.
    /// </summary>
    public sealed partial record BurnsSamusInHeat : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static BurnsSamusInHeat Instance { get; } = new();
    }
}
