using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The selection missile's roll, shared by file select, options and game over (#2359), played by the shared animation
/// player: its pictures in turn, each for the menu's installed hold (#1596), over and over. The roll starts on its first
/// picture, one update from its second.
/// </summary>
internal sealed partial class MenuMissileAnimation
{
    /// <summary>The roll as it plays.</summary>
    internal readonly AnimationPlayer<MenuMissileSegment, MenuMissileFrame, NoAnimationCue> player;

    /// <summary>Starts the roll on its first picture, each later picture held for <paramref name="frameDuration"/> updates.</summary>
    internal MenuMissileAnimation(int frameDuration)
    {
        AnimationSegments<MenuMissileSegment, MenuMissileFrame, NoAnimationCue> segments = Segments(frameDuration);
        player = AnimationPlayer<MenuMissileSegment, MenuMissileFrame, NoAnimationCue>.StartShowing(segments, Rolling(segments), 1);
    }

    /// <summary>The picture the missile shows.</summary>
    internal MenuMissileFrame Frame => player.Showing;

    /// <summary>Runs one update of the roll.</summary>
    internal void Step() => player.Step();

    /// <summary>
    /// Holds each picture the roll moves on to for the rebound installed <paramref name="frameDuration"/>, keeping the
    /// picture shown and its updates left.
    /// </summary>
    internal void Rebind(int frameDuration)
    {
        AnimationSegments<MenuMissileSegment, MenuMissileFrame, NoAnimationCue> segments = Segments(frameDuration);
        player.Rebind(segments, Rolling(segments));
    }

    /// <summary>The roll's one segment: each picture in turn, held for <paramref name="frameDuration"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameDuration"/> is below one.</exception>
    private static AnimationSegments<MenuMissileSegment, MenuMissileFrame, NoAnimationCue> Segments(int frameDuration)
    {
        if (frameDuration < 1)
            throw new ArgumentOutOfRangeException(nameof(frameDuration), frameDuration, "The missile holds each picture for at least one update.");
        return new(new Dictionary<MenuMissileSegment, AnimationBeat<MenuMissileFrame, NoAnimationCue>[]>
        {
            [MenuMissileSegment.Roll] =
                [.. MenuMissileAnimationDefinitions.Roll.Select(frame => new AnimationBeat<MenuMissileFrame, NoAnimationCue>.Frame(frame, frameDuration))],
        });
    }

    /// <summary>The roll played over and over.</summary>
    private static Animation<MenuMissileSegment> Rolling(AnimationSegments<MenuMissileSegment, MenuMissileFrame, NoAnimationCue> segments) =>
        Animation<MenuMissileSegment>.Looping([segments.Whole(MenuMissileSegment.Roll)]);
}
