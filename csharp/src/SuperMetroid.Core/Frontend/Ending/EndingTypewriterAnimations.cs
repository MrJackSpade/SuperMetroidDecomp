using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A post-credits text's typing (#2488), played by the shared animation player as the completion texts type
/// (<see cref="EndingTextAnimations"/>): its opening pause, each installed character typed for
/// <see cref="EndingTypewriterDefinitions.CharacterDelayFrames"/> updates, for the item percentage its percentage held for
/// <see cref="EndingTypewriterDefinitions.PercentageHoldFrames"/>, then its completion, resting on the whole text. The
/// text's characters are installed, so its typing is built for the text it types.
/// </summary>
internal static partial class EndingTypewriterAnimations
{
    /// <summary>The typing of <paramref name="sequence"/>, whose installed characters are <paramref name="characters"/>.</summary>
    internal static AnimationSegments<EndingTextSequence, EndingTypewriterTyping, EndingTypewriterCue> Segments(EndingTextSequence sequence,
        IReadOnlyCollection<EndingTextCharacter> characters) =>
        new(new Dictionary<EndingTextSequence, AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>[]>
        {
            [sequence] = Typed(sequence, Ensure.NotNull(characters).Count),
        }, [sequence]);

    /// <summary>The animation that types <paramref name="sequence"/> once and rests on what it typed.</summary>
    internal static Animation<EndingTextSequence> Typing(
        AnimationSegments<EndingTextSequence, EndingTypewriterTyping, EndingTypewriterCue> segments, EndingTextSequence sequence) =>
        new([Ensure.NotNull(segments).Whole(sequence)], AnimationEnd.Holds);

    /// <summary>
    /// The beats of <paramref name="sequence"/>'s typing: its opening pause, each character, the item percentage's hold, then
    /// its completion.
    /// </summary>
    private static AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>[] Typed(EndingTextSequence sequence, int characters)
    {
        var beats = new List<AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>>
        {
            new AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>.Pause(EndingTypewriterDefinitions.InitialDelayFrames),
        };
        beats.AddRange(Enumerable.Range(1, characters).Select(typed =>
            new AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>.Frame(new(typed), EndingTypewriterDefinitions.CharacterDelayFrames)));
        if (EndingTextSequenceDefinitions.HoldingPercentage.Contains(sequence))
        {
            beats.Add(new AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>.Event(EndingTypewriterCue.ShowPercentage));
            beats.Add(new AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>.Pause(EndingTypewriterDefinitions.PercentageHoldFrames));
        }
        beats.Add(new AnimationBeat<EndingTypewriterTyping, EndingTypewriterCue>.Event(EndingTypewriterCue.Complete));
        return [.. beats];
    }
}
