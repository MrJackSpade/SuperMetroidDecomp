using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="CrateriaLightningPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CrateriaLightningPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by the production definition: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<CrateriaLightningPaletteFxColorDefinition, (int ColorsPerFrame, int NeutralCount)> CrossChecks = new()
    {
        [CrateriaLightningPaletteFxColorDefinitions.Surface] = (8, 1),
    };

    /// <summary>Exposes the lightning's definition and the complete neutral-flash-recovery cycle lengths.</summary>
    extension(CrateriaLightningPaletteFxColorDefinition self)
    {
        /// <summary>Live colours per timed record: the eight the lightning flashes.</summary>
        internal int ColorsPerFrame => CrossChecks[self].ColorsPerFrame;
        /// <summary>The 240-update neutral records the program shows before its last flash group.</summary>
        internal int NeutralCount => CrossChecks[self].NeutralCount;
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
