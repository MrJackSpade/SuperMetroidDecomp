namespace SuperMetroid.Core.Assets;

/// <summary>The piece and first character that identify a cached single-piece frame.</summary>
/// <param name="Part">The piece, drawn small at the origin.</param>
/// <param name="FirstCharacter">Sheet character of the bound artwork's character 0.</param>
public readonly record struct SmallObjectKey(SpritePart Part, int FirstCharacter);
