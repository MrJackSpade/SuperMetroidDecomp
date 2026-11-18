using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using G = SuperMetroid.Core.Samus.Colours.SamusGlowSegment;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// Plays Samus's named shade sequences (<see cref="SamusGlowAnimationDefinitions"/>) through the shared animation player
/// (#2387): a colouring that shows a shade on every update plays its sequence on from the place it keeps, and one that holds
/// each shade for its updates keeps a player that counts them.
/// </summary>
internal static class SamusGlowPlay
{
    /// <summary>A player of <paramref name="sequence"/> that shows nothing until its first update shows the first shade.</summary>
    internal static AnimationPlayer<G, PictureShade, NoAnimationCue> Starting(G sequence) =>
        AnimationPlayer<G, PictureShade, NoAnimationCue>.Starting(
            SamusGlowAnimationDefinitions.Segments, SamusGlowAnimationDefinitions.Animations[sequence]);

    /// <summary>Starts <paramref name="player"/> on <paramref name="sequence"/> again, showing nothing until its next update.</summary>
    internal static void Restart(AnimationPlayer<G, PictureShade, NoAnimationCue> player, G sequence) =>
        Ensure.NotNull(player).Start(SamusGlowAnimationDefinitions.Animations[sequence]);

    /// <summary>Plays <paramref name="sequence"/> on from <paramref name="place"/> to its next shade, and moves the place past it.</summary>
    /// <exception cref="InvalidOperationException">The sequence ends instead of showing a shade.</exception>
    internal static AnimationShown<G, PictureShade>.Frame NextShade(G sequence, ref AnimationPlace place)
    {
        AnimationShown<G, PictureShade> next = SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(
            SamusGlowAnimationDefinitions.Animations[sequence], ref place, static _ => { });
        return next as AnimationShown<G, PictureShade>.Frame
            ?? throw new InvalidOperationException($"The {sequence} sequence reached {next}; it only shows shades.");
    }

    /// <summary>
    /// Plays <paramref name="sequence"/> to its next shade going on from <paramref name="playback"/>, and keeps where it stands
    /// past it: a playback of another sequence hands it its place (<see cref="AnimationPlayback{TSegment}.GoingOnIn"/>), as
    /// the Screw Attack and Speed Booster glows share theirs and every suit's glow goes on from another's.
    /// </summary>
    /// <exception cref="InvalidOperationException">The sequence ends instead of showing a shade.</exception>
    internal static AnimationShown<G, PictureShade>.Frame NextShade(G sequence, ref AnimationPlayback<G> playback)
    {
        Animation<G> animation = SamusGlowAnimationDefinitions.Animations[sequence];
        AnimationPlace place = ReferenceEquals(playback.Animation, animation) ? playback.Place : playback.GoingOnIn(animation).Place;
        AnimationShown<G, PictureShade>.Frame shown = NextShade(sequence, ref place);
        playback = new(animation, place);
        return shown;
    }

    /// <summary>The playback of <paramref name="sequence"/> before its first shade.</summary>
    internal static AnimationPlayback<G> Before(G sequence) =>
        new(SamusGlowAnimationDefinitions.Animations[sequence], AnimationPlace.StartOfAnimation);
}
