using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Hyper Beam colour effect, the projectile colour cycle
/// started when Samus receives the Hyper Beam.
/// </summary>
/// <remarks>
/// The state retains the animation position and timer. Fixed animation control is
/// compiled separately from the live colours so presentation replacement cannot
/// silently change engine cadence or route restored state into adjacent game data.
/// </remarks>
public sealed class HyperBeamColourEffectState
{
    /// <summary>Where the Hyper Beam's colour animation resumes between updates.</summary>
    internal AnimationPlace Place { get; private set; }

    /// <summary>True after equipping the Hyper Beam has spawned the object.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The current shade's remaining hold. Spawn initializes it to one; the effect
    /// decrements before testing, so the first shade shows on its first update.
    /// </summary>
    public int FrameHold { get; private set; }

    /// <summary>
    /// Starts the Hyper Beam's colour effect. No colour is shown until its first update
    /// (<see cref="Step"/>).
    /// </summary>
    public void Spawn()
    {
        // Allocation clears the effect's colour step and auxiliary timer, installs timer
        // one, then calls the object's no-op setup. This specialized state has no
        // reusable fields to clear, but preserves every observable field and delay.
        IsActive = true;
        FrameHold = 1;
        Place = AnimationPlace.StartOfAnimation;
    }

    /// <summary>Runs one update of the Hyper Beam's colour effect.</summary>
    public void Step(SamusColours colours)
    {
        Ensure.NotNull(colours);

        if (!IsActive)
        {
            return;
        }

        // The effect decrements before testing. A timer of two therefore displays a frame
        // on the starting update and one following update, then advances on the third update.
        FrameHold--;
        if (FrameHold != 0)
        {
            return;
        }

        AnimationPlace place = Place;
        AnimationShown<SamusGlowSegment, PictureShade> next = SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(
            SamusGlowAnimationDefinitions.Animations[SamusGlowSegment.HyperBeamCycle], ref place, static _ => { });
        Place = place;
        if (next is not AnimationShown<SamusGlowSegment, PictureShade>.Frame shown)
            throw new InvalidOperationException($"The Hyper Beam's colour cycle reached {next}; it only shows shades.");

        colours.LayOverBeams(shown.Shown);

        FrameHold = shown.Duration;
        _ = shown.Shown;
    }

}
