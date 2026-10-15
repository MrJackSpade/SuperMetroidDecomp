
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="PaletteFxHeatInstructionListDefinitions"/> members production does not use.</summary>
internal static class PaletteFxHeatInstructionListDefinitionsAccess
{
    /// <summary>Resolves the native selector-word source for each heat-suit and phase pair.</summary>
    extension(PaletteFxHeatInstructionListDefinitions)
    {
        /// <summary>Returns the native bank-$8D selector-word address for parity verification.</summary>
        internal static ushort NativeSourceAddress(PaletteFxHeatSuit suit, int phase)
        {
            if ((uint)phase >= PaletteFxHeatInstructionListDefinitions.PhaseCount)
                throw new ArgumentOutOfRangeException(nameof(phase));

            ushort table = suit switch
            {
                PaletteFxHeatSuit.Power => PaletteFxHeatInstructionListDefinitions.PowerSourceTable,
                PaletteFxHeatSuit.Varia => PaletteFxHeatInstructionListDefinitions.VariaSourceTable,
                PaletteFxHeatSuit.Gravity => PaletteFxHeatInstructionListDefinitions.GravitySourceTable,
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
            };
            return unchecked((ushort)(table + phase * sizeof(ushort)));
        }
    }
}
