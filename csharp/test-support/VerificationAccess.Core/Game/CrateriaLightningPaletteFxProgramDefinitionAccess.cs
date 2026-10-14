using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CrateriaLightningPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class CrateriaLightningPaletteFxProgramDefinitionAccess
{
    /// <summary>Exposes surface/dark lightning identity and the complete neutral-flash-recovery cycle lengths.</summary>
    extension(CrateriaLightningPaletteFxProgramDefinition self)
    {
        /// <summary>$8D:F765 live surface-lightning definition or $F769 unused dark-lightning definition.</summary>
        internal ushort DefinitionPointer => self.IsSurface ? (ushort)0xf765 : (ushort)0xf769;

        /// <summary>Full lightning cycle including neutral holds, two flashes, and recovery.</summary>
        internal int CycleFrames => 240 * (1 + self.NeutralCount) + 2 * 9 + 5;

        /// <summary>Displayed records across neutral, active, and recovery phases.</summary>
        internal int DisplayedRecordsPerCycle => 1 + 14 + self.NeutralCount + 4 + 1;
    }
}
