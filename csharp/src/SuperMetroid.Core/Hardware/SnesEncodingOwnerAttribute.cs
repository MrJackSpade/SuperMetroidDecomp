namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Marks the type that owns and documents an encoding (#223): its byte order, storage widths, fixed-point layout,
/// bank/offset split or packed fields. Inside it, hexadecimal literals and width shifts and masks are the
/// definition itself; everywhere else call sites use its named operations, and the cartridge-encoding ratchet
/// (SME6340, SME6345) reports raw ones.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class SnesEncodingOwnerAttribute : Attribute;
