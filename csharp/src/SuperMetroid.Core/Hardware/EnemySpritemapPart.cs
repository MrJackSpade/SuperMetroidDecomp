namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One five-byte enemy OBJ visual record after decoding. Its origin, palette selection,
/// base tile and OAM wrapping remain caller-owned, not editable animation mechanics.
/// </summary>
public readonly record struct EnemySpritemapPart(
    SnesSpritemapXWord X, byte Y, SnesObjAttributeWord Attributes);
