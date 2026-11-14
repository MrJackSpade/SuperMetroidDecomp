namespace SuperMetroid.Core.Input;

/// <summary>One recorded input of a demo (#1300, #1600): the buttons held and the separately authored edges, for a duration.</summary>
/// <param name="Held">The buttons held.</param>
/// <param name="NewlyPressed">The buttons published as newly pressed, independent of <paramref name="Held"/>.</param>
/// <param name="Duration">
/// The updates the input lasts, 0 through 65535. A zero duration lasts 65,536 updates: the recording's countdown wraps
/// below zero, so a recording that held an input that long would replay it exactly.
/// </param>
public readonly record struct DemoInput(ControllerButton Held, ControllerButton NewlyPressed, int Duration);
