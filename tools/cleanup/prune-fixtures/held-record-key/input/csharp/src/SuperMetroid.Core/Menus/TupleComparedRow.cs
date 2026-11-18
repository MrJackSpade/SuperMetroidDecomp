namespace SuperMetroid.Core.Menus;

/// <summary>A row the host compares together with a page by tuple equality.</summary>
/// <param name="Row">The row's place, which no code names: tuple equality compares it.</param>
public sealed record TupleComparedRow(int Row);
