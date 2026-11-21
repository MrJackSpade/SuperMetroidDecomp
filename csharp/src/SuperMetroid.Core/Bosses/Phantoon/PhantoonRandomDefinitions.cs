namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The random choices of Phantoon's fight: its timers, patterns and figure-eight starts.</summary>
internal static class PhantoonRandomDefinitions
{
    /// <summary>
    /// How long an opened eye tracks Samus (#2510): the vulnerable-window timer's eight picks
    /// (<see cref="PhantoonTimerDefinitions.VulnerableWindow"/>), taken in turn by the draw's eight values.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> VulnerableWindow = PhantoonTimerDefinitions.VulnerableWindow.ChosenBy(RandomSampleField.OneOf(8));

    /// <summary>The figure eight after the eye closes running reversed (#2470): one time in two.</summary>
    internal static readonly RandomChance FigureEightRunsReversed = RandomChance.Highest(1, RandomSampleField.OneOf(2));

    /// <summary>The next figure eight starting on the right, at <see cref="PhantoonPathDefinitions.RightAppearancePosition"/> (#2470): one time in two.</summary>
    internal static readonly RandomChance FigureEightStartsRight = RandomChance.Highest(1, RandomSampleField.OneOf(2));

    /// <summary>
    /// How long Phantoon hides before its flame rain (#2510): the hiding timer's eight picks
    /// (<see cref="PhantoonTimerDefinitions.RainHiding"/>), taken in turn by the draw's eight values.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> RainHiding = PhantoonTimerDefinitions.RainHiding.ChosenBy(RandomSampleField.OneOf(8));

    /// <summary>
    /// How long the second round's pattern keeps the eye closed (#2510): the eye-closed timer's eight picks
    /// (<see cref="PhantoonTimerDefinitions.EyeClosed"/>), taken in turn by the draw's eight values.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> EyeClosed = PhantoonTimerDefinitions.EyeClosed.ChosenBy(RandomSampleField.OneOf(8));
}
