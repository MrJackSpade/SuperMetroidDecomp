namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One five-byte enemy OBJ visual record after decoding. Its origin, palette selection,
/// base tile and OAM wrapping remain caller-owned, not editable animation mechanics.
/// </summary>
/// <param name="X">The encoded X offset and size selector.</param>
/// <param name="Y">The encoded signed Y offset.</param>
/// <param name="Attributes">The tile number and OBJ attributes.</param>
public readonly record struct EnemySpritemapPart(
    SnesSpritemapXWord X, SnesSpritemapYByte Y, SnesObjAttributeWord Attributes)
{
    /// <summary>Decodes a record whose Y field is the cartridge record's encoded byte.</summary>
    /// <param name="x">The encoded X offset and size selector.</param>
    /// <param name="encodedY">The record's encoded Y byte, the displacement modulo 256.</param>
    /// <param name="attributes">The tile number and OBJ attributes.</param>
    public EnemySpritemapPart(SnesSpritemapXWord x, byte encodedY, SnesObjAttributeWord attributes)
        : this(x, new SnesSpritemapYByte(encodedY), attributes)
    {
    }
}
