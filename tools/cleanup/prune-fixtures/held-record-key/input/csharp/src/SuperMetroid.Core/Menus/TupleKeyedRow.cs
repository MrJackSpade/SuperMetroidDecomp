namespace SuperMetroid.Core.Menus;

/// <summary>A row the host keys its anchors by together with a page, in a tuple.</summary>
/// <param name="Row">The row's place, which no code names: hashing the tuple key compares it.</param>
public sealed record TupleKeyedRow(int Row);
