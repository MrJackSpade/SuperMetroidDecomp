namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What Mother Brain's brain animation shows (#2317, #2554): the picture it last showed, or none yet, from the start of an
/// animation that opens with a cue until its first draw plays that cue and the picture after it.
/// </summary>
public abstract record MotherBrainBrainShown
{
    /// <summary>Closes the hierarchy to the cases declared here.</summary>
    private MotherBrainBrainShown()
    {
    }

    /// <summary>The animation shows <paramref name="Frame"/>.</summary>
    /// <param name="Frame">The picture shown.</param>
    public sealed record Picture(EnemyFrameId Frame) : MotherBrainBrainShown;

    /// <summary>
    /// The animation has shown no picture yet: it opens with a cue, which its first draw plays before its first picture.
    /// Drawn with time frozen in this state, the game reads the word after the opening cue as the brain's picture, which is
    /// none of the brain's pictures; the brain shows nothing then (a presentation quirk the port does not reproduce).
    /// </summary>
    public sealed record NotYetShown : MotherBrainBrainShown;

    /// <summary>The state of an animation that has shown no picture yet.</summary>
    public static MotherBrainBrainShown Nothing { get; } = new NotYetShown();
}
