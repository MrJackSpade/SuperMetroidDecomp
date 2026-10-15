using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Development-tool instance members of <see cref="RoomPaletteFxPresentation"/>.</summary>
internal static class RoomPaletteFxPresentationToolingExtensions
{
    /// <summary>Inspects installed palette identities for the development dependency audit.</summary>
    extension(RoomPaletteFxPresentation self)
    {
        /// <summary>
        /// Installed identities, including shared-color aliases, for the development
        /// dependency auditor. This exposes keys only, not cartridge bytes or colors.
        /// </summary>
        internal IEnumerable<ushort> ColorPointers
        {
            get
            {
                foreach (ushort pointer in self.colors.Keys)
                    if (!MaridiaEnvironmentalColorDefinitions.TrySourcePointer(pointer, out _) &&
                        !PlanetZebesTextColorDefinitions.TryCoordinates(pointer, out _, out _, out _) &&
                        !CrateriaLightningColorDefinitions.TryCoordinates(pointer, out _, out _) &&
                        !CrateriaLightningColorDefinitions.TryDarkCoordinates(pointer, out _, out _) &&
                        !HeatPaletteColorDefinitions.TryCanonicalPointer(pointer, out _) &&
                        !LoadingPaletteColorDefinitions.TryCanonicalPointer(pointer, out _) &&
                        !LogoGlarePaletteColorDefinitions.TryCoordinates(pointer, out _, out _) &&
                        !EndingGunshipPaletteColorDefinitions.TryCoordinates(pointer, out _, out _) &&
                        !TourianStatueGreyColorDefinitions.TryCoordinates(pointer, out _, out _)) yield return pointer;
                foreach (var program in MaridiaEnvironmentalPaletteFxColorDefinitions.All)
                    for (int frame = 0; frame < program.FrameCount; frame++)
                        for (int index = 0; index < program.ColorsPerFrame; index++)
                            yield return program.ColorPointer(frame, index);
                foreach (var program in PlanetZebesTextPaletteFxColorDefinitions.All)
                    for (int frame = 0; frame < PlanetZebesTextPaletteFxColorDefinitions.FrameCount; frame++)
                        for (int index = 0; index < PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame; index++)
                            yield return program.ColorPointer(frame, index);
                for (int frame = 0; frame < CrateriaLightningColorDefinitions.DarkProgram.Frames.Count; frame++)
                    for (int index = 0; index < CrateriaLightningColorDefinitions.DarkProgram.ColorsPerFrame; index++)
                        yield return CrateriaLightningColorDefinitions.DarkProgram.ColorPointer(frame, index);
                for (int frame = 0; frame < CrateriaLightningColorDefinitions.SurfaceProgram.Frames.Count; frame++)
                    for (int index = 0; index < CrateriaLightningColorDefinitions.SurfaceProgram.ColorsPerFrame; index++)
                        yield return CrateriaLightningColorDefinitions.SurfaceProgram.ColorPointer(frame, index);
                for (int frame = 0; frame < TourianStatueGreyPaletteFxColorDefinitions.FrameCount; frame++)
                    for (int index = 0; index < TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame; index++)
                        yield return TourianStatueGreyPaletteFxColorDefinitions.ColorPointer(frame, index);
                for (int frame = 0; frame < ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount; frame++)
                    for (int index = 0; index < ZebesExplosionGunshipPaletteFxColorDefinitions.ColorsPerFrame; index++)
                        yield return ZebesExplosionGunshipPaletteFxColorDefinitions.ColorPointer(frame, index);
                for (int frame = 0; frame < PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount; frame++)
                    for (int index = 0; index < PostCreditsIconGlarePaletteFxColorDefinitions.ColorsPerFrame; index++)
                        yield return PostCreditsIconGlarePaletteFxColorDefinitions.ColorPointer(frame, index);
                foreach (var program in SamusLoadingSuitPaletteFxColorDefinitions.All)
                for (int frame = 0; frame < 9; frame++)
                    for (int index = 0; index < 16; index++) yield return program.ColorPointer(frame, index);
                foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
                foreach (var frame in program.Frames)
                    for (int index = 0; index < 15; index++) yield return (ushort)(frame.FirstColorPointer + 2 * index);
            }
        }
    }
}
