namespace SuperMetroid.Core.Menus;

/// <summary>A row the host compares with <see cref="object.Equals(object, object)"/>.</summary>
/// <param name="Row">The row's place, which no code names: the comparison calls the record's equality, which compares it.</param>
public sealed record StaticComparedRow(int Row);
