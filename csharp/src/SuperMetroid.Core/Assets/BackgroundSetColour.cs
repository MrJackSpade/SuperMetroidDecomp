namespace SuperMetroid.Core.Assets;

/// <summary>One colour of a named gameplay background set (#1464).</summary>
/// <param name="Set">The set holding the colour.</param>
/// <param name="Colour">The colour within the set.</param>
public readonly record struct BackgroundSetColour(BackgroundColourSet Set, int Colour);
