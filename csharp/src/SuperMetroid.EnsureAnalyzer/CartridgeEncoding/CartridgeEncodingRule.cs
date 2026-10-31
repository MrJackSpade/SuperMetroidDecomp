namespace SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

/// <summary>What a cartridge-encoding occurrence is, for the ratchet and the #641 inventory.</summary>
internal enum CartridgeEncodingRule
{
    /// <summary>A hexadecimal literal outside an encoding owner, catalogs included (#1480): ratcheted by SME6340.</summary>
    RawHexLiteral,
    /// <summary>
    /// A primitive compared with, offset from or switched over an address-valued code or instruction
    /// constant: ratcheted by SME6341.
    /// </summary>
    PointerDispatch,
    /// <summary>An integral array or collection literal holding hexadecimal words: ratcheted by SME6342.</summary>
    OpaqueWordArray,
    /// <summary>A hexadecimal literal inside a <c>[EncodingOwner]</c> codec, which owns the encoding. Inventoried.</summary>
    OwnedHexLiteral,
    /// <summary>A functional hexadecimal literal carrying a reviewed same-line allowance. Inventoried, not ratcheted.</summary>
    JustifiedHexLiteral,
    /// <summary>
    /// A raw storage-width or layout operation in functional code (#223): a shift by a byte, word or bank width,
    /// or a byte/word/bank mask. Ratcheted by SME6345.
    /// </summary>
    RawWidthOperation,
    /// <summary>A width operation inside a catalog or a <c>[EncodingOwner]</c> type that documents it. Inventoried.</summary>
    OwnedWidthOperation,
    /// <summary>A functional width operation carrying a reviewed same-line allowance. Inventoried, not ratcheted.</summary>
    JustifiedWidthOperation,
    /// <summary>
    /// A hand-written sign-bit test on a countdown-named primitive in functional code (#1324): <c>&amp; 0x8000</c>,
    /// <c>&amp; 0x80</c>, a comparison with <c>0x8000</c> or <c>0x80</c>, or <c>(short)x &lt; 0</c> and its
    /// <c>(sbyte)</c> and <c>&gt;= 0</c> forms. Ratcheted by SME6348.
    /// </summary>
    CountdownSignTest,
    /// <summary>
    /// A wrapping self-decrement of 16-bit or 8-bit storage in functional code (#1324):
    /// <c>x = unchecked((ushort)(x - 1))</c>, its <c>byte</c> form, or <c>--</c>/<c>-= 1</c> on a <c>ushort</c> or
    /// <c>byte</c> field, property or element. Ratcheted by SME6349.
    /// </summary>
    CountdownSelfDecrement,
    /// <summary>
    /// A value of a <c>[Flags]</c> enum cast to an integral primitive in functional code (#1323): the typed flags
    /// leave their type before the encoding boundary. Ratcheted by SME6346.
    /// </summary>
    FlagsPrimitiveCast,
    /// <summary>A <c>[Flags]</c> cast inside a catalog or <c>[EncodingOwner]</c> type, or under a reviewed allowance. Inventoried.</summary>
    OwnedFlagsPrimitiveCast,
    /// <summary>
    /// A <c>byte</c> or <c>ushort</c> instance field, property or positional record parameter in functional code
    /// (#1323): gameplay state stored as a CPU word. Ratcheted by SME6347.
    /// </summary>
    NarrowStorageMember,
    /// <summary>A narrow member inside a catalog or <c>[EncodingOwner]</c> type, or under a reviewed allowance. Inventoried.</summary>
    OwnedNarrowStorageMember,
    /// <summary>
    /// A cast to <c>byte</c>, <c>sbyte</c>, <c>short</c> or <c>ushort</c> outside a <c>[EncodingOwner]</c> type and the
    /// Compatibility module (#1480): an inline wrap or sign reinterpretation. Ratcheted by SME6453.
    /// </summary>
    NarrowingCast,
    /// <summary>A narrowing cast carrying a reviewed same-line allowance. Inventoried, not ratcheted.</summary>
    JustifiedNarrowingCast,
}
