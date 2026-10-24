namespace SuperMetroid.Core.Hardware;

/// <summary>A source the sprite characters show, with its place among all placements ever shown (#1406).</summary>
/// <param name="Source">The source shown.</param>
/// <param name="Stamp">Its place among all placements ever shown; a later placement has a larger stamp.</param>
public readonly record struct ShownSpriteCharacters(ObjCharacterSource Source, long Stamp);
