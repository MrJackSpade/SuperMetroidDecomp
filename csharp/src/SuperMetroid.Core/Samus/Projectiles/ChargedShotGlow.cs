using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The glow Samus's suit shows after a charged shot (#1682, #2351), each kind a named shade sequence played through the
/// shared animation player (<see cref="SamusGlowPlay"/>, #2458): an ordinary charged shot's flash shows the yellow glow,
/// and the Hyper Beam shot's glow shows the Hyper Beam's full-body glow from its last frame to its first
/// (<see cref="SamusGlowAnimationDefinitions"/>). On the update after its last shade the glow ends and the suit's own shade returns.
/// </summary>
/// <remarks>
/// The glow's kind decides how it plays. The game decided by whether the Hyper Beam is equipped, and only a Hyper
/// Beam glow ever runs while it is: the Hyper Beam fires no ordinary charged shot and is never unequipped.
/// </remarks>
public sealed class ChargedShotGlow
{
    /// <summary>Starts a glow of <paramref name="kind"/>, showing nothing until its first update shows its first shade.</summary>
    private ChargedShotGlow(ChargedShotGlowKind kind, SamusGlowSegment sequence)
    {
        Kind = kind;
        Play = SamusGlowPlay.Starting(sequence);
    }

    /// <summary>An ordinary charged shot's flash as the shot starts it.</summary>
    public static ChargedShotGlow StartChargedShotFlash() => new(ChargedShotGlowKind.ChargedShotFlash, SamusGlowSegment.ChargedShotFlash);

    /// <summary>The Hyper Beam's glow as its shot starts it.</summary>
    public static ChargedShotGlow StartHyperBeamGlow() => new(ChargedShotGlowKind.HyperBeamGlow, SamusGlowSegment.HyperShotGlow);

    /// <summary>Which glow this is.</summary>
    private ChargedShotGlowKind Kind { get; }

    /// <summary>The play of the glow's sequence: the shade it shows and the updates that shade has left.</summary>
    private AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> Play { get; }

    /// <summary>Whether the glow has played past its last shade, so the suit's own shade returns.</summary>
    internal bool HasEnded => Play.HasStopped;

    /// <summary>
    /// Plays the glow one update on and returns the shade the update shows anew: its next shade once the shade shown last
    /// has held its updates, or none while that shade holds and on the update the glow ends (<see cref="HasEnded"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The glow has ended.</exception>
    internal PictureShade? PlayUpdate()
    {
        if (HasEnded)
            throw new InvalidOperationException($"The {Kind} glow has ended and plays no further.");
        return Play.StepToNewFrame();
    }
}
