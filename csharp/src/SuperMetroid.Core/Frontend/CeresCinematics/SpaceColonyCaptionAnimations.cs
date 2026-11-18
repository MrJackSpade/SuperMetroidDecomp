using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The SPACE COLONY caption's typing (#2488), played by the shared animation player as the title cards type: each letter
/// shows for <see cref="CeresFlightSceneDefinitions.CaptionLetterDelay"/> updates, with Japanese text the katakana caption
/// then shows for <see cref="CeresFlightSceneDefinitions.JapaneseCaptionDelay"/>, the whole caption holds for
/// <see cref="CeresFlightSceneDefinitions.CaptionHold"/>, and the flight then fades out.
/// </summary>
internal static class SpaceColonyCaptionAnimations
{
    /// <summary>Each caption's typing.</summary>
    internal static AnimationSegments<SpaceColonyCaptionText, SpaceColonyCaptionTyping, SpaceColonyCaptionCue> Segments { get; } = new(
        new Dictionary<SpaceColonyCaptionText, AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>[]>
        {
            [SpaceColonyCaptionText.English] = [.. Letters(), .. Held()],
            [SpaceColonyCaptionText.WithKatakana] =
            [
                .. Letters(),
                new AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>.Frame(
                    new(SpaceColonyCaptionDefinitions.LetterCount, SpaceColonyCaptionText.WithKatakana), CeresFlightSceneDefinitions.JapaneseCaptionDelay),
                .. Held(),
            ],
        });

    /// <summary>The animation that types <paramref name="caption"/> once and rests on it.</summary>
    internal static Animation<SpaceColonyCaptionText> Typing(SpaceColonyCaptionText caption) =>
        new([Segments.Whole(caption)], AnimationEnd.Holds);

    /// <summary>The caption's letters, each shown for the letter delay.</summary>
    private static IEnumerable<AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>> Letters() =>
        Enumerable.Range(1, SpaceColonyCaptionDefinitions.LetterCount).Select(letters =>
            new AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>.Frame(new(letters, SpaceColonyCaptionText.English), CeresFlightSceneDefinitions.CaptionLetterDelay));

    /// <summary>The whole caption's hold, then the cue that fades the flight out.</summary>
    private static AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>[] Held() =>
    [
        new AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>.Pause(CeresFlightSceneDefinitions.CaptionHold),
        new AnimationBeat<SpaceColonyCaptionTyping, SpaceColonyCaptionCue>.Event(SpaceColonyCaptionCue.StartFadeOut),
    ];
}
