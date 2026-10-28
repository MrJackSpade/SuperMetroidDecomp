namespace SuperMetroid.Core.Assets;

/// <summary>One piece of a frontend or projectile sprite composition, and whether it takes the draw's palette.</summary>
/// <param name="Part">The piece: offset, size, character, mirroring, palette row and priority.</param>
/// <param name="InheritPalette">Whether the piece draws in the palette its draw selects instead of its own row.</param>
internal readonly record struct CompiledSpritePart(SpritePart Part, bool InheritPalette);
