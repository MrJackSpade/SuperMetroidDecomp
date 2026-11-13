namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// What a running colour effect watches each update before its countdown runs (#1554). A program step names the
/// condition (<see cref="PaletteFxCue.Watch"/>); an effect watches nothing until one does, and watches it until it ends.
/// </summary>
public abstract record ColourEffectCondition
{
    /// <summary>Closes the hierarchy to the conditions declared here.</summary>
    private ColourEffectCondition()
    {
    }

    /// <summary>
    /// While Samus stands higher in the room than <see cref="Height"/>, the effect restarts <see cref="Restart"/> on
    /// every update, so it shows that program's first record each update instead of its flashes: the Crateria
    /// lightning stays calm while Samus is high on the surface.
    /// </summary>
    /// <param name="Height">The room height, in whole pixels, Samus must be above.</param>
    /// <param name="Restart">The program the effect restarts while she is.</param>
    public sealed record RestartsWhileSamusIsHigh(int Height, PaletteFxSegment Restart) : ColourEffectCondition;

    /// <summary>
    /// The effect ends on the first update the Torizo whose belly it lights, the room's first enemy, has no health left.
    /// </summary>
    public sealed record EndsWhenTorizoDies : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static EndsWhenTorizoDies Instance { get; } = new();
    }

    /// <summary>The effect ends on the first update the area's mini-boss is recorded dead: Spore Spawn's spores.</summary>
    public sealed record EndsWhenAreaMiniBossDies : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static EndsWhenAreaMiniBossDies Instance { get; } = new();
    }

    /// <summary>
    /// Samus's heat shimmer: each update the heat burns a Samus without a heat-protecting suit, and the shimmer restarts
    /// on the heat phase the Norfair glow published when it changed.
    /// </summary>
    public sealed record BurnsSamusInHeat : ColourEffectCondition
    {
        /// <summary>The one condition.</summary>
        public static BurnsSamusInHeat Instance { get; } = new();
    }
}
