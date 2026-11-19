using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionAmbientPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<ZebesExplosionAmbientPaletteFxProgramOwner, (ColourEffectId Object, int ColorsPerFrame, int CycleFrames)> CrossChecks = new()
    {
        [ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow] = (ColourEffectId.ZebesExplosionPlanetAfterglow, 8, 96),
        [ZebesExplosionAmbientPaletteFxProgramOwner.Lava] = (ColourEffectId.ExplodingZebesLava, 1, 70),
    };

    /// <summary>Exposes <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner].Object;
        /// <summary>The number of live BGR555 colors written by each timed record.</summary>
        internal int ColorsPerFrame => CrossChecks[self.Owner].ColorsPerFrame;
        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames => CrossChecks[self.Owner].CycleFrames;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
        /// <summary>The number of timed records in the loop.</summary>
        public int FrameCount => self.ColorFrames.FrameCount;
    }
}
