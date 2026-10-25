namespace SuperMetroid.Core;

/// <summary>
/// Marks the type that owns and documents one encoding (#223, #1420): an installed file, a save, a recording or another
/// stored form whose byte order, storage widths, fixed-point layout or packed fields the type defines. Inside it,
/// hexadecimal literals and width shifts and masks are the definition itself; everywhere else call sites use its named
/// operations, and the cartridge-encoding ratchet (SME6340, SME6345, SME6348, SME6349) reports raw ones. The format
/// argument names the encoding the type owns.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class EncodingOwnerAttribute : Attribute
{
    /// <summary>Marks the single owner of <paramref name="format"/>.</summary>
    /// <param name="format">The file or stored form the type owns, named for the game.</param>
    public EncodingOwnerAttribute(string format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        Format = format;
    }

    /// <summary>The file or stored form the type owns.</summary>
    public string Format { get; }
}
