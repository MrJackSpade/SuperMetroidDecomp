/// <summary>
/// Retail room and door identities used by the Kraid end-to-end audit. These are kept
/// separate from the audit behavior so call sites state which cartridge object they use
/// instead of relying on raw addresses and adjacent explanatory comments.
/// </summary>
internal static class KraidAuditDefinitions
{
    /// <summary>Kraid-room door <c>$83:91CE</c>, which exits left after the fight.</summary>
    public const ushort LeftExitDoor = 0x91ce;

    /// <summary>Destination room <c>$8F:A56B</c> of Kraid's left exit.</summary>
    public const ushort LeftExitDestination = 0xa56b;

    /// <summary>Kraid-room door <c>$83:91DA</c>, which exits right after the fight.</summary>
    public const ushort RightExitDoor = 0x91da;

    /// <summary>Destination room <c>$8F:A6E2</c> of Kraid's right exit.</summary>
    public const ushort RightExitDestination = 0xa6e2;
}
