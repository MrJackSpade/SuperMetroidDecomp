using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The glow Samus's suit shows after a charged shot (#1682, #2351), each kind a named shade sequence played as an
/// animation (<see cref="SamusGlowAnimationDefinitions"/>): an ordinary charged shot's flash shows the yellow glow for
/// three updates, and the Hyper Beam shot's glow shows the Hyper Beam's full-body glow from its last frame to its first,
/// each for two updates. On the update after its last shade the glow ends and the suit's own shade returns.
/// </summary>
/// <remarks>
/// The glow's kind decides how it plays. The game decided by whether the Hyper Beam is equipped, and only a Hyper
/// Beam glow ever runs while it is: the Hyper Beam fires no ordinary charged shot and is never unequipped.
/// </remarks>
public readonly record struct ChargedShotGlow
{
    /// <summary>Creates a glow of <paramref name="kind"/> standing at <paramref name="place"/>, its shade held <paramref name="hold"/> more updates.</summary>
    private ChargedShotGlow(ChargedShotGlowKind kind, AnimationPlace place, int hold)
    {
        Kind = kind;
        Place = place;
        Hold = Ensure.AtLeastZero(hold);
    }

    /// <summary>No glow.</summary>
    public static ChargedShotGlow None => default;

    /// <summary>An ordinary charged shot's flash as the shot starts it.</summary>
    public static ChargedShotGlow ChargedShotFlash { get; } = new(ChargedShotGlowKind.ChargedShotFlash, AnimationPlace.StartOfAnimation, 0);

    /// <summary>The Hyper Beam's glow as its shot starts it.</summary>
    public static ChargedShotGlow HyperBeamGlow { get; } = new(ChargedShotGlowKind.HyperBeamGlow, AnimationPlace.StartOfAnimation, 0);

    /// <summary>Which glow this is.</summary>
    public ChargedShotGlowKind Kind { get; }

<<<<<<< HEAD
    /// <summary>Where the glow's sequence stands: the start before its first update, then just past the shade it showed last.</summary>
    internal AnimationPlace Place { get; }
||||||| 273b556990
    /// <summary>The steps the glow has left; see <see cref="Of"/> for each kind's range.</summary>
    [IntegerQuantity("charge colour calls the glow has left")]
    public int RemainingSteps { get; }
=======
    /// <summary>The steps the glow has left; see <see cref="Of"/> for each kind's range.</summary>
    [IntegerQuantity("updates of the charge colour the glow has left")]
    public int RemainingSteps { get; }
>>>>>>> worktree-agent-a25205e6d337c0739

    /// <summary>The updates the shade shown last still holds for, the one that showed it included; zero before the first update.</summary>
    internal int Hold { get; }

    /// <summary>
    /// The glow one update on, and the shade it shows on that update: its next shade once the shade shown last has held
    /// its updates, or none while that shade holds. A glow past its last shade is <see cref="None"/>.
    /// </summary>
    /// <param name="shown">The shade the update shows, or null when it shows none.</param>
    /// <exception cref="InvalidOperationException">There is no glow to play.</exception>
    internal ChargedShotGlow Played(out PictureShade? shown)
    {
        if (Hold > 1)
        {
            shown = null;
            return new(Kind, Place, Hold - 1);
        }
        SamusGlowSegment sequence = Kind switch
        {
            ChargedShotGlowKind.ChargedShotFlash => SamusGlowSegment.ChargedShotFlash,
            ChargedShotGlowKind.HyperBeamGlow => SamusGlowSegment.HyperShotGlow,
            ChargedShotGlowKind.None => throw new InvalidOperationException("No charged shot's glow is playing."),
            _ => throw new InvalidOperationException($"Undefined {nameof(ChargedShotGlowKind)} {Kind}."),
        };
        AnimationPlace place = Place;
        AnimationShown<SamusGlowSegment, PictureShade> next = SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(
            SamusGlowAnimationDefinitions.Animations[sequence], ref place, static _ => { });
        switch (next)
        {
            case AnimationShown<SamusGlowSegment, PictureShade>.Frame frame:
                shown = frame.Shown;
                return new(Kind, place, frame.Duration);
            case AnimationShown<SamusGlowSegment, PictureShade>.Ended:
                shown = null;
                return None;
            default:
                throw new InvalidOperationException($"The {sequence} sequence reached {next}; a shot's glow shows shades and ends.");
        }
    }
}
