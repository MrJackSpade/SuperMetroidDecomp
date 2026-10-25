namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values and helpers of <see cref="SamusEaterPlmDefinitions"/> that only verification reads.</summary>
internal static class SamusEaterPlmDefinitionsConstants
{
    /// <summary>$84:AC89, pin Samus to saved coordinates and OR immunity with $10.</summary>
    public const ushort HoldPreInstruction = 0xac89;
}
