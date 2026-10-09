namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One five-byte enemy OBJ visual record after decoding. Its origin, palette selection,
/// base tile and OAM wrapping remain caller-owned, not editable animation mechanics.
/// </summary>
/// <param name="X">Encoded modular X offset and independent large-object selector.</param>
/// <param name="Y">Unsigned byte Y offset retained from the spritemap record.</param>
/// <param name="Attributes">Packed tile number, palette, priority, and flip bits.</param>
public readonly record struct EnemySpritemapPart(
    SnesSpritemapXWord X, byte Y, SnesObjAttributeWord Attributes);
