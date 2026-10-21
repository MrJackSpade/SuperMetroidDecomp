namespace SuperMetroid.Core.Saves;

/// <summary>A save field's value and its JSON property name.</summary>
/// <param name="Value">The value.</param>
/// <param name="Property">The property.</param>
internal readonly record struct SaveFieldCheck(int Value, string Property);
