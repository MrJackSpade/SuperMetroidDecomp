using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool instance members of <see cref="RoomPaletteFxPresentation"/>.</summary>
internal static class RoomPaletteFxPresentationToolingExtensions
{
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
                foreach (var program in MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
                    for (int frame = 0; frame < program.FrameCount; frame++)
                        for (int index = 0; index < program.ColorsPerFrame; index++)
                            yield return program.ColorPointer(frame, index);
                foreach (var program in PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
                    for (int frame = 0; frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
                        for (int index = 0; index < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
                            yield return program.ColorPointer(frame, index);
                for (int frame = 0; frame < CrateriaLightningColorDefinitions.DarkProgram.Frames.Count; frame++)
                    for (int index = 0; index < CrateriaLightningColorDefinitions.DarkProgram.ColorsPerFrame; index++)
                        yield return CrateriaLightningColorDefinitions.DarkProgram.ColorPointer(frame, index);
                for (int frame = 0; frame < CrateriaLightningColorDefinitions.SurfaceProgram.Frames.Count; frame++)
                    for (int index = 0; index < CrateriaLightningColorDefinitions.SurfaceProgram.ColorsPerFrame; index++)
                        yield return CrateriaLightningColorDefinitions.SurfaceProgram.ColorPointer(frame, index);
                for (int frame = 0; frame < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
                    for (int index = 0; index < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
                        yield return TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
                for (int frame = 0; frame < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
                    for (int index = 0; index < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
                        yield return ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
                for (int frame = 0; frame < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
                    for (int index = 0; index < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
                        yield return PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
                foreach (var program in SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
                for (int frame = 0; frame < 9; frame++)
                    for (int index = 0; index < 16; index++) yield return program.ColorPointer(frame, index);
                foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
                foreach (var frame in program.Frames)
                    for (int index = 0; index < 15; index++) yield return (ushort)(frame.FirstColorPointer + 2 * index);
            }
        }
    }
}
