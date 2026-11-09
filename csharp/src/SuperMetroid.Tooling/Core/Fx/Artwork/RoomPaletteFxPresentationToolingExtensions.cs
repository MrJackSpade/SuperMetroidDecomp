using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Development-tool instance members of <see cref="RoomPaletteFxPresentation"/>.</summary>
internal static class RoomPaletteFxPresentationToolingExtensions
{
    /// <summary>Inspects installed palette identities for the development dependency audit.</summary>
    extension(RoomPaletteFxPresentation self)
    {
        /// <summary>
        /// The bank-$8D addresses of the installed color keys, for the development dependency auditor. This exposes
        /// identities only, not cartridge bytes or colors.
        /// </summary>
        internal IEnumerable<ushort> ColorPointers => self.ColorKeys.Select(PaletteFxRecordSources.ColorAddress);

        /// <summary>Installed color identities, including shared-color aliases.</summary>
        internal IEnumerable<PaletteFxColorKey> ColorKeys
        {
            get
            {
                foreach (PaletteFxColorKey key in self.colors.Keys)
                    if (!MaridiaEnvironmentalColors.TrySource(key, out _) &&
                        !PlanetZebesTextColors.TryCoordinates(key, out _, out _, out _) &&
                        !CrateriaLightningColorDefinitions.SurfaceProgram.ColorFrames.TryLocate(key, out _, out _) &&
                        !CrateriaLightningColorDefinitions.DarkProgram.ColorFrames.TryLocate(key, out _, out _) &&
                        !HeatPaletteColors.TryCanonical(key, out _) &&
                        !LoadingPaletteColors.TryCanonical(key, out _) &&
                        !PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames.TryLocate(key, out _, out _) &&
                        !ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames.TryLocate(key, out _, out _) &&
                        !TourianStatueGreyPaletteFxColorDefinitions.ColorFrames.TryLocate(key, out _, out _)) yield return key;
                foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.ColorFrames)) yield return key;
                foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.ColorFrames)) yield return key;
                foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.DarkProgram.ColorFrames)) yield return key;
                foreach (PaletteFxColorKey key in Keys(CrateriaLightningColorDefinitions.SurfaceProgram.ColorFrames)) yield return key;
                foreach (PaletteFxColorKey key in Keys(TourianStatueGreyPaletteFxColorDefinitions.ColorFrames)) yield return key;
                foreach (PaletteFxColorKey key in Keys(ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames)) yield return key;
                foreach (PaletteFxColorKey key in Keys(PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames)) yield return key;
                foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.ColorFrames)) yield return key;
                foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
                    foreach (PaletteFxColorKey key in Keys(program.ColorFrames)) yield return key;
            }
        }
    }

    /// <summary>Every color of <paramref name="colorFrames"/>, by frame then color.</summary>
    private static IEnumerable<PaletteFxColorKey> Keys(PaletteFxColorFrames colorFrames)
    {
        for (int frame = 0; frame < colorFrames.FrameCount; frame++)
            for (int index = 0; index < colorFrames.ColorsPerFrame; index++)
                yield return colorFrames.Color(frame, index);
    }
}
