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

    /// <summary>
    /// Starts <paramref name="player"/> on <paramref name="sequence"/> again, showing nothing until its
    /// <paramref name="updates"/>th update (#2447).
    /// </summary>
    internal static void RestartAfter(AnimationPlayer<G, PictureShade, NoAnimationCue> player, G sequence, int updates) =>
        Ensure.NotNull(player).StartAfter(SamusGlowAnimationDefinitions.Animations[sequence], updates);

    /// <summary>
    /// Starts <paramref name="player"/> on <paramref name="sequence"/> again on its first shade, which shows at once for
    /// <paramref name="updates"/> updates (#2447).
    /// </summary>
    /// <returns>The first shade, which the player now shows.</returns>
    internal static PictureShade RestartShowing(AnimationPlayer<G, PictureShade, NoAnimationCue> player, G sequence, int updates) =>
        Ensure.NotNull(player).Show(SamusGlowAnimationDefinitions.Animations[sequence], updates);

    /// <summary>
    /// Counts <paramref name="player"/>'s shown shade down and, once it runs out, plays <paramref name="sequence"/> on to its
    /// next shade from where the player stands, going on in it from another sequence's place (#2447,
    /// <see cref="AnimationPlayer{TSegment, TFrame, TEvent}.StepToNewFrameGoingOnIn"/>).
    /// </summary>
    /// <returns>The shade the update shows anew, or null while the shown shade holds.</returns>
    internal static PictureShade? StepToNextShadeGoingOnIn(AnimationPlayer<G, PictureShade, NoAnimationCue> player, G sequence) =>
        Ensure.NotNull(player).StepToNewFrameGoingOnIn(SamusGlowAnimationDefinitions.Animations[sequence]);

    /// <summary>Plays <paramref name="sequence"/> on from <paramref name="place"/> to its next shade, and moves the place past it.</summary>
    /// <exception cref="InvalidOperationException">The sequence ends instead of showing a shade.</exception>
    internal static AnimationShown<G, PictureShade>.Frame NextShade(G sequence, ref AnimationPlace place)
    {
        AnimationShown<G, PictureShade> next = SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(
            SamusGlowAnimationDefinitions.Animations[sequence], ref place, static _ => { });
        return next is AnimationShown<G, PictureShade>.Frame frame
            ? frame
            : throw new InvalidOperationException($"The {sequence} sequence reached {next}; it only shows shades.");
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
