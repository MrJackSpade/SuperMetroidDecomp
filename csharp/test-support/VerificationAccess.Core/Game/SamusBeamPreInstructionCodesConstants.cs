using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SamusBeamPreInstructionCodes"/> that only verification reads.</summary>
internal static class SamusBeamPreInstructionCodesConstants
{
    /// <summary>$90:BA3E, FireChargedBeam's low-nibble-indexed callback words.</summary>
    public const int ChargedTable = 0x90ba3e;
    /// <summary>$90:B96E, FireUnchargedBeam's low-nibble-indexed callback words.</summary>
    public const int UnchargedTable = 0x90b96e;

    /// <summary>Provides charged and uncharged beam pre-instruction tables for projectile behavior checks.</summary>
    extension(SamusBeamPreInstructionCodes)
    {
        /// <inheritdoc cref="SamusBeamPreInstructionCodesConstants.ChargedTable"/>
        internal static int ChargedTable => SamusBeamPreInstructionCodesConstants.ChargedTable;
        /// <inheritdoc cref="SamusBeamPreInstructionCodesConstants.UnchargedTable"/>
        internal static int UnchargedTable => SamusBeamPreInstructionCodesConstants.UnchargedTable;
    }
}
