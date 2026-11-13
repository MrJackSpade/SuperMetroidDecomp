using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="CrateriaLightningPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CrateriaLightningPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes the lightning's definition and the complete neutral-flash-recovery cycle lengths.</summary>
    extension(CrateriaLightningPaletteFxColorDefinition self)
    {
        /// <summary>The $8D:F765 surface-lightning definition.</summary>
        internal static ushort DefinitionPointer => 0xf765;

        /// <summary>Full lightning cycle including neutral holds, two flashes, and recovery.</summary>
        internal int CycleFrames => 240 * (1 + self.NeutralCount) + 2 * 9 + 5;

        /// <summary>Displayed records across neutral, active, and recovery phases.</summary>
        internal int DisplayedRecordsPerCycle => 1 + 14 + self.NeutralCount + 4 + 1;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
