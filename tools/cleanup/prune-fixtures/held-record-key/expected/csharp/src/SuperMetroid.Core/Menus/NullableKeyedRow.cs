namespace SuperMetroid.Core.Menus;

/// <summary>A row the host keeps in a set of rows that may be absent.</summary>
/// <param name="Row">The row's place, which no code names: hashing the nullable row compares it.</param>
public readonly record struct NullableKeyedRow(int Row);
