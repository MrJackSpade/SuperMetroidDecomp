namespace SuperMetroid.Core.Input;

/// <summary>One demo input record's controller words (#1300): the held buttons and the separately authored edges.</summary>
/// <param name="Held">The buttons held.</param>
/// <param name="NewlyPressed">The buttons published as newly pressed, independent of <paramref name="Held"/>.</param>
public readonly record struct DemoInputFrame(SnesButton Held, SnesButton NewlyPressed);
