using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Ending.EndingTextTyping, SuperMetroid.Core.Frontend.Ending.EndingSceneCue>;
using D = SuperMetroid.Core.Frontend.Ending.EndingTypedTextDefinitions;
using K = SuperMetroid.Core.Frontend.Ending.EndingSceneCue;
using T = SuperMetroid.Core.Frontend.Ending.EndingTypedText;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The animations of the ending's typed texts (#1513, #2195, #2197): each types its text one letter a step, as a title
/// card does, its frames the text typed so far, then publishes the cues that follow it and rests on the whole text.
/// </summary>
internal static class EndingTextAnimations
{
    /// <summary>The sprite family whose sheet pictures the typed texts' letters: the result text's.</summary>
    internal const EndingSpriteFamily Family = EndingSpriteFamily.EndingCompletionText;

    /// <summary>The typed texts' animations.</summary>
    internal static CinematicAnimations<EndingTypedText, EndingTextTyping, EndingSceneCue> Texts { get; } = Create();

    /// <summary>How many letters <paramref name="text"/> types: its spelling's letters, without its spaces.</summary>
    private static int Letters(EndingTypedText text) => D.Spellings[text].Count(letter => letter != ' ');

    /// <summary>What <paramref name="text"/> shows as it types, in typing order: its text typed as far as each letter.</summary>
    internal static IReadOnlyList<EndingTextTyping> Typings(EndingTypedText text) =>
        [.. Enumerable.Range(1, Letters(text)).Select(letters => new EndingTextTyping(text, letters))];

    /// <summary>
    /// Writes each text's animation: its typing, then THE OPERATION WAS spawns COMPLETED SUCCESSFULLY, which spawns CLEAR
    /// TIME, which spawns the clear time's columns one by one and, once it has shown the whole clear time, moves on to
    /// the credits; each then rests on its whole text.
    /// </summary>
    private static CinematicAnimations<EndingTypedText, EndingTextTyping, EndingSceneCue> Create()
    {
        var segments = new Dictionary<EndingTypedText, AnimationBeat<EndingTextTyping, EndingSceneCue>[]>
        {
            [T.OperationWas] = [.. Typing(T.OperationWas), B.Raise(K.SpawnCompletedText), B.Hold(Whole(T.OperationWas))],
            [T.CompletedSuccessfully] = [.. Typing(T.CompletedSuccessfully), B.Raise(K.SpawnClearTime), B.Hold(Whole(T.CompletedSuccessfully))],
            [T.ClearTime] =
            [
                .. Typing(T.ClearTime),
                B.Raise(K.SpawnHoursTens),
                B.Show(Whole(T.ClearTime), D.ClearTimeColumnUpdates),
                B.Raise(K.SpawnHoursUnits),
                B.Show(Whole(T.ClearTime), D.ClearTimeColumnUpdates),
                B.Raise(K.SpawnColon),
                B.Show(Whole(T.ClearTime), D.ClearTimeColumnUpdates),
                B.Raise(K.SpawnMinutesTens),
                B.Show(Whole(T.ClearTime), D.ClearTimeColumnUpdates),
                B.Raise(K.SpawnMinutesUnits),
                B.Show(Whole(T.ClearTime), D.ClearTimeShownUpdates),
                B.Raise(K.TransitionToCredits),
                B.Hold(Whole(T.ClearTime)),
            ],
        };
        return new(segments, plays => segments.Keys.ToDictionary(text => text, text => plays.Through(text, AnimationEnd.Holds)));
    }

    /// <summary><paramref name="text"/> typed in full.</summary>
    private static EndingTextTyping Whole(EndingTypedText text) => new(text, Letters(text));

    /// <summary>
    /// <paramref name="text"/>'s typing: each letter shows for <see cref="D.LetterUpdates"/>, the last before a space or the
    /// text's end for <see cref="D.LastLetterUpdates"/>, in which the space after it is typed.
    /// </summary>
    private static IEnumerable<AnimationBeat<EndingTextTyping, EndingSceneCue>> Typing(EndingTypedText text)
    {
        string spelling = D.Spellings[text];
        int letters = 0;
        for (int place = 0; place < spelling.Length; place++)
        {
            if (spelling[place] == ' ')
                continue;
            letters++;
            bool pauses = place + 1 == spelling.Length || spelling[place + 1] == ' ';
            yield return B.Show(new EndingTextTyping(text, letters), pauses ? D.LastLetterUpdates : D.LetterUpdates);
        }
    }
}
