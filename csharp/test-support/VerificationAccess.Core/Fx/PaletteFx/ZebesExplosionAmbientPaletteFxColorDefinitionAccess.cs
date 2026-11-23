using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionAmbientPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
        /// <summary>The number of timed records in the loop.</summary>
        public int FrameCount => self.ColorFrames.FrameCount;
        /// <summary>The live colours each timed record writes.</summary>
        internal int ColorsPerFrame => self.ColorFrames.ColorsPerFrame;
        /// <summary>The native loop duration in updates: 96 for the afterglow, 70 for the lava.</summary>
        internal int CycleFrames => self.Effect == ColourEffectId.ZebesExplosionPlanetAfterglow ? 96 : 70;
    }
}
