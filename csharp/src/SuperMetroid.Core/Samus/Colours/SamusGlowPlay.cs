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

    /// <summary>
    /// Plays <paramref name="sequence"/> on from <paramref name="place"/> to its next shade, moves the place past it, and shows
    /// the shade on <paramref name="colours"/>' suit; a sequence at rest or over shows nothing new, the suit keeping its shade.
    /// </summary>
    internal static void ShowNextShade(G sequence, ref AnimationPlace place, SamusColours colours)
    {
<<<<<<< ours
        AnimationShown<G, PictureShade> next = SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(
            SamusGlowAnimationDefinitions.Animations[sequence], ref place, static _ => { });
        return next is AnimationShown<G, PictureShade>.Frame frame
            ? frame
            : throw new InvalidOperationException($"The {sequence} sequence reached {next}; it only shows shades.");
||||||| base
        AnimationShown<G, PictureShade> next = SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(
            SamusGlowAnimationDefinitions.Animations[sequence], ref place, static _ => { });
        return next as AnimationShown<G, PictureShade>.Frame
            ?? throw new InvalidOperationException($"The {sequence} sequence reached {next}; it only shows shades.");
=======
        Ensure.NotNull(colours);
        SamusGlowAnimationDefinitions.Segments.PlayToNextFrame(SamusGlowAnimationDefinitions.Animations[sequence], ref place, static _ => { })
            .React(ShowNextShadeReactions, colours);
>>>>>>> theirs
    }

    /// <summary>What <see cref="ShowNextShade(G, ref AnimationPlace, SamusColours)"/> does on each <see cref="AnimationShown{G, PictureShade}"/>, built once (#2553).</summary>
    private static AnimationShownReactions<G, PictureShade, SamusColours> ShowNextShadeReactions { get; } = new(
            Frame: static (shade, colours) => colours.ShowSuit(shade.Shown),
            HeldFrame: static (shade, colours) => colours.ShowSuit(shade.Shown),
            Paused: static (_, _) => { },
            Held: static (_, _) => { },
            Ended: static (_, _) => { });

    /// <summary>
    /// Plays <paramref name="sequence"/> to its next shade going on from <paramref name="playback"/>, and keeps where it stands
    /// past it: a playback of another sequence hands it its place (<see cref="AnimationPlayback{TSegment}.GoingOnIn"/>), as
    /// the Screw Attack and Speed Booster glows share theirs and every suit's glow goes on from another's.
    /// </summary>
    internal static void ShowNextShade(G sequence, ref AnimationPlayback<G> playback, SamusColours colours)
    {
        Animation<G> animation = SamusGlowAnimationDefinitions.Animations[sequence];
        AnimationPlace place = ReferenceEquals(playback.Animation, animation) ? playback.Place : playback.GoingOnIn(animation).Place;
        ShowNextShade(sequence, ref place, colours);
        playback = new(animation, place);
    }

    /// <summary>The playback of <paramref name="sequence"/> before its first shade.</summary>
    internal static AnimationPlayback<G> Before(G sequence) =>
        new(SamusGlowAnimationDefinitions.Animations[sequence], AnimationPlace.StartOfAnimation);
}
