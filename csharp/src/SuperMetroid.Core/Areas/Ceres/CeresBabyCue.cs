namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres Baby's animations: what it does, with the values it acts on.</summary>
internal abstract record CeresBabyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresBabyCue()
    {
    }

    /// <summary>
    /// The baby reacts to its motion (#2167): while its vertical velocity is nonzero its owner starts its expressive loop.
    /// </summary>
    internal sealed record ReactsToMotion : CeresBabyCue;

    /// <summary>Shows one of the Baby's shades.</summary>
    /// <param name="Shade">The shade an <see cref="UpdateColors"/> cue shows.</param>
    internal sealed record UpdateColors(CeresBabyShade Shade) : CeresBabyCue;

    /// <summary>
    /// The baby cries (#2167); while it is still, one time in two by the game's current random number, it instead rests
    /// silently, its owner starting its resting loop.
    /// </summary>
    internal sealed record Cries : CeresBabyCue;
}
