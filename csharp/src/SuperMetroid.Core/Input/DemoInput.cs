namespace SuperMetroid.Core.Input;

/// <summary>One recorded input of a demo (#1300, #1600): the actions held and the separately authored edges, for a duration.</summary>
/// <param name="Held">The actions held.</param>
/// <param name="NewlyPressed">The actions published as newly pressed, independent of <paramref name="Held"/>.</param>
/// <param name="Duration">The updates the input lasts, at least one; a recording refuses any shorter input.</param>
public readonly record struct DemoInput(GameplayAction Held, GameplayAction NewlyPressed, int Duration);
