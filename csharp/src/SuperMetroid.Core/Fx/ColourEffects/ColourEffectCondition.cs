namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// What a running colour effect watches each update before its countdown runs (#1554, #1982). The effect's definition
/// names the condition (<see cref="ColourEffectDefinition"/>); the effect watches it from the update after it shows its
/// first frame until it ends.
/// </summary>
public abstract record ColourEffectCondition
{
    /// <summary>Closes the hierarchy to the conditions declared here.</summary>
    private ColourEffectCondition()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this colour effect condition, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(ColourEffectConditionReactions reactions);


    /// <summary>
    /// The effect watches nothing (#2544): the condition of an effect whose definition names none, and of every effect before
    /// it shows its first frame.
    /// </summary>
    public sealed record WatchesNothing : ColourEffectCondition
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectConditionReactions reactions) => Ensure.NotNull(reactions).WatchesNothing(this);

        /// <summary>The one condition.</summary>
        public static WatchesNothing Instance { get; } = new();
    }

    /// <summary>
    /// While Samus stands higher in the room than <see cref="Height"/>, the effect restarts its animation on every update, so
    /// it shows its first frame, the calm the lightning loops from, each update instead of its flashes: the Crateria
    /// lightning stays calm while Samus is high on the surface.
    /// </summary>
    /// <param name="Height">The room height, in whole pixels, Samus must be above.</param>
    public sealed record RestartsWhileSamusIsHigh(int Height) : ColourEffectCondition
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectConditionReactions reactions) => Ensure.NotNull(reactions).RestartsWhileSamusIsHigh(this);
    }

    /// <summary>
    /// The effect ends on the first update the Torizo whose belly it lights (<see cref="ILitTorizo"/>), the Torizo whose
    /// fight start started it, has no health left.
    /// </summary>
    public sealed record EndsWhenTorizoDies : ColourEffectCondition
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectConditionReactions reactions) => Ensure.NotNull(reactions).EndsWhenTorizoDies(this);

        /// <summary>The one condition.</summary>
        public static EndsWhenTorizoDies Instance { get; } = new();
    }

    /// <summary>The effect ends on the first update the area's mini-boss is recorded dead: Spore Spawn's spores.</summary>
    public sealed record EndsWhenAreaMiniBossDies : ColourEffectCondition
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectConditionReactions reactions) => Ensure.NotNull(reactions).EndsWhenAreaMiniBossDies(this);

        /// <summary>The one condition.</summary>
        public static EndsWhenAreaMiniBossDies Instance { get; } = new();
    }

    /// <summary>
    /// Samus's heat shimmer: each update the heat burns a Samus without a heat-protecting suit, and the shimmer restarts
    /// on the heat phase the Norfair glow published when it changed.
    /// </summary>
    public sealed record BurnsSamusInHeat : ColourEffectCondition
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectConditionReactions reactions) => Ensure.NotNull(reactions).BurnsSamusInHeat(this);

        /// <summary>The one condition.</summary>
        public static BurnsSamusInHeat Instance { get; } = new();
    }
}

/// <summary>What the owner of a <see cref="ColourEffectCondition"/> does on each of its cases (#2553).</summary>
/// <param name="WatchesNothing">Reacts to a <see cref="ColourEffectCondition.WatchesNothing"/>.</param>
/// <param name="RestartsWhileSamusIsHigh">Reacts to a <see cref="ColourEffectCondition.RestartsWhileSamusIsHigh"/>.</param>
/// <param name="EndsWhenTorizoDies">Reacts to a <see cref="ColourEffectCondition.EndsWhenTorizoDies"/>.</param>
/// <param name="EndsWhenAreaMiniBossDies">Reacts to a <see cref="ColourEffectCondition.EndsWhenAreaMiniBossDies"/>.</param>
/// <param name="BurnsSamusInHeat">Reacts to a <see cref="ColourEffectCondition.BurnsSamusInHeat"/>.</param>
public sealed record ColourEffectConditionReactions(Action<ColourEffectCondition.WatchesNothing> WatchesNothing, Action<ColourEffectCondition.RestartsWhileSamusIsHigh> RestartsWhileSamusIsHigh, Action<ColourEffectCondition.EndsWhenTorizoDies> EndsWhenTorizoDies, Action<ColourEffectCondition.EndsWhenAreaMiniBossDies> EndsWhenAreaMiniBossDies, Action<ColourEffectCondition.BurnsSamusInHeat> BurnsSamusInHeat);
