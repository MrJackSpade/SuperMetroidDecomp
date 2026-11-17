namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One step of a typed narration letter's glow (#2361): the letter is typed in the first step's colours, and its glow then
/// shows each step in turn, leaving the letter in the last step's colours.
/// </summary>
public readonly record struct IntroTextGlowStep
{
    /// <summary>The steps of the glow.</summary>
    private const int StepCount = 4;

    /// <summary>The step at <paramref name="step"/> of the glow.</summary>
    private IntroTextGlowStep(int step) => Step = step;

    /// <summary>The step's place in the glow, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>Every step of the glow, in the order it shows them.</summary>
    public static IReadOnlyList<IntroTextGlowStep> All { get; } = [.. Enumerable.Range(0, StepCount).Select(step => new IntroTextGlowStep(step))];

    /// <summary>The glow's first step, the colours a letter is typed in.</summary>
    public static IntroTextGlowStep Typed => All[0];

    /// <summary>The step after this one, or null after the glow's last, where the letter keeps its colours.</summary>
    public IntroTextGlowStep? Following => Step + 1 < StepCount ? All[Step + 1] : null;
}
