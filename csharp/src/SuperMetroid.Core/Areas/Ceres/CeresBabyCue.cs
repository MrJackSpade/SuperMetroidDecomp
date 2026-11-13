namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres Baby's animations: what it does, with the values it acts on.</summary>
internal abstract record CeresBabyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresBabyCue()
    {
    }

    /// <summary>
    /// Continues at the target while the baby's vertical velocity is nonzero.
    /// </summary>
    /// <param name="Target">The segment a branch continues at.</param>
    internal sealed record GoToIfMoving(CeresBabySegment Target) : CeresBabyCue;

    /// <summary>Shows one of the Baby's shades.</summary>
    /// <param name="Shade">The shade an <see cref="UpdateColors"/> cue shows.</param>
    internal sealed record UpdateColors(CeresBabyShade Shade) : CeresBabyCue;

    /// <summary>
    /// Cries, or, while the baby is still and the game's current random number is odd, continues silently at the target.
    /// </summary>
    /// <param name="Target">The segment a branch continues at.</param>
    internal sealed record CryOrGoTo(CeresBabySegment Target) : CeresBabyCue;
}
