namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Where a live eye beam stands in its lifetime (#2554), with the step the beam takes there on each update: only the
/// four stages below exist, so the beam's update has a step for every stage it can be in.
/// </summary>
internal abstract class MorphBallEyeBeamStage
{
    /// <summary>Only the four stages below derive from this.</summary>
    private MorphBallEyeBeamStage() { }

    /// <summary>The lifetime phase the stage shows as.</summary>
    internal abstract MorphBallEyeBeamPhase Phase { get; }

    /// <summary>Takes the beam's step in this stage for <paramref name="cast"/>, which <paramref name="species"/> casts.</summary>
    internal abstract void Step(MorphBallEyeBehavior species, MorphBallEyeBeamCast cast);

    /// <summary>Requested, which the next beam update starts.</summary>
    internal static MorphBallEyeBeamStage PendingInitialization { get; } = new Requested();

    /// <summary>Widening ever faster toward its full half-width.</summary>
    internal static MorphBallEyeBeamStage Widening { get; } = new Widen();

    /// <summary>Spread its full half-width, cycling its colours while its eye stays activated.</summary>
    internal static MorphBallEyeBeamStage Full { get; } = new Spread();

    /// <summary>Fading toward black once its eye has closed.</summary>
    internal static MorphBallEyeBeamStage Deactivating { get; } = new Fade();

    /// <summary>The requested beam, which its first update starts.</summary>
    private sealed class Requested : MorphBallEyeBeamStage
    {
        /// <inheritdoc/>
        internal override MorphBallEyeBeamPhase Phase => MorphBallEyeBeamPhase.PendingInitialization;

        /// <inheritdoc/>
        internal override void Step(MorphBallEyeBehavior species, MorphBallEyeBeamCast cast) => species.StartMorphBallEyeBeam(cast);
    }

    /// <summary>The widening beam.</summary>
    private sealed class Widen : MorphBallEyeBeamStage
    {
        /// <inheritdoc/>
        internal override MorphBallEyeBeamPhase Phase => MorphBallEyeBeamPhase.Widening;

        /// <inheritdoc/>
        internal override void Step(MorphBallEyeBehavior species, MorphBallEyeBeamCast cast) => species.WidenMorphBallEyeBeam(cast);
    }

    /// <summary>The full beam.</summary>
    private sealed class Spread : MorphBallEyeBeamStage
    {
        /// <inheritdoc/>
        internal override MorphBallEyeBeamPhase Phase => MorphBallEyeBeamPhase.Full;

        /// <inheritdoc/>
        internal override void Step(MorphBallEyeBehavior species, MorphBallEyeBeamCast cast) => species.StepFullMorphBallEyeBeam(cast);
    }

    /// <summary>The fading beam.</summary>
    private sealed class Fade : MorphBallEyeBeamStage
    {
        /// <inheritdoc/>
        internal override MorphBallEyeBeamPhase Phase => MorphBallEyeBeamPhase.Deactivating;

        /// <inheritdoc/>
        internal override void Step(MorphBallEyeBehavior species, MorphBallEyeBeamCast cast) => species.FadeMorphBallEyeBeam();
    }
}
