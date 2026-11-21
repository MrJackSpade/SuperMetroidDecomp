using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of Mother Brain's colour fades playing (#2251, #2404): the shared animation player showing the fade's images, each
/// for its hold. Each fade keeps its own: greying at her fake death, the return from that grey, greying as the Baby drains
/// her, her revival from that grey, her final death's body fade, her corpse's greying, the Baby's fade to black and the room
/// lights' recovery. The phase that runs a fade waits on its end: the update after its last image's hold, which shows no
/// new colours.
/// </summary>
public sealed class MotherBrainFadeProgress
{
    /// <summary>The player showing the fade's images.</summary>
    internal AnimationPlayer<MotherBrainFadeSegment, MotherBrainFadeImage, NoAnimationCue> Player { get; private set; }

    /// <summary>Readies <paramref name="fade"/> before its start: its first update shows its first image.</summary>
    internal MotherBrainFadeProgress(MotherBrainFade fade)
    {
        Fade = Ensure.NotNull(fade);
        Player = AnimationPlayer<MotherBrainFadeSegment, MotherBrainFadeImage, NoAnimationCue>.Starting(fade.Segments, fade.Animation);
    }

    /// <summary>The fade playing.</summary>
    internal MotherBrainFade Fade { get; }

    /// <summary>Whether the fade is over: its last image has shown its hold out.</summary>
    public bool IsOver => Player.HasStopped;

    /// <summary>Starts the fade over, showing its first image on the <paramref name="updates"/>th update from now.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal void StartAfter(int updates) =>
        Player = AnimationPlayer<MotherBrainFadeSegment, MotherBrainFadeImage, NoAnimationCue>.StartingAfter(Fade.Segments, Fade.Animation, updates);

    /// <summary>
    /// Runs one update of the fade: the image it shows anew on this update, or null while an image holds, before the first
    /// and once the fade is over.
    /// </summary>
    internal MotherBrainFadeImage? Step() => Player.StepToNewFrame();
}
