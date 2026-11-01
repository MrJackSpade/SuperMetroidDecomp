using System.Collections.Frozen;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads and writes the installed room palette-effect colour document.</summary>
public static class RoomPaletteFxPresentationFile
{
    /// <summary>The revision each accepted stored version number names; the stored number is decoded once, here.</summary>
    private static readonly FrozenDictionary<int, RoomPaletteFxPresentationRevision> Revisions =
        new Dictionary<int, RoomPaletteFxPresentationRevision>
        {
            [RoomPaletteFxPresentationFormat.PreviousVersion] = RoomPaletteFxPresentationRevision.InheritsHeatRows,
            [RoomPaletteFxPresentationFormat.Version] = RoomPaletteFxPresentationRevision.Current,
        }.ToFrozenDictionary();

    /// <summary>Validates editable palette-FX frames and compiles their RGB5 colors into the native bank-$8D color lookup.</summary>
    /// <param name="json">The UTF-8 JSON presentation stream with the required frame counts and RGB components from zero through thirty-one.</param>
    /// <param name="previousVersionFallback">Current stock colors used only to supply the missing Samus heat frames when loading the previous schema version.</param>
    /// <returns>A color source preserving independent edits while reconstructing matching shared or calculated samples on demand.</returns>
    /// <exception cref="InvalidDataException">The schema version, required frame geometry, or a color component is invalid, or the fallback lacks a required heat color.</exception>
    public static RoomPaletteFxPresentation Read(Stream json,
        RoomPaletteFxPresentation? previousVersionFallback = null)
    {
        RoomPaletteFxPresentationDocument document = JsonAssetDocument.Read<RoomPaletteFxPresentationDocument>(
            json, ArtworkJson.Options, "room palette-FX presentation");

        bool supported = Revisions.TryGetValue(document.Version, out RoomPaletteFxPresentationRevision revision);
        bool inheritsHeatRows = supported && revision == RoomPaletteFxPresentationRevision.InheritsHeatRows;
        if (!supported || (inheritsHeatRows && previousVersionFallback is null))
        {
            throw new InvalidDataException(
                $"Room palette-FX presentation requires version " +
                $"{RoomPaletteFxPresentationFormat.Version}.");
        }

        var colors = new Dictionary<PaletteFxColorKey, RgbColor>();
        foreach (PaletteFxHeatProgramDefinition definition in
                 PaletteFxHeatProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Suit switch
            {
                PaletteFxHeatSuit.Power => document.SamusHeatPowerSuit,
                PaletteFxHeatSuit.Varia => document.SamusHeatVariaSuit,
                PaletteFxHeatSuit.Gravity => document.SamusHeatGravitySuit,
                _ => throw new InvalidDataException($"Unsupported heat suit {definition.Suit}."),
            };
            if (inheritsHeatRows)
            {
                // A v17 player override predates these three color families. Keep
                // all its existing edits and inherit only the newly extracted
                // Samus-in-heat frames from the verified current stock catalog.
                for (int frame = 0; frame < definition.ColorFrames.FrameCount; frame++)
                for (int index = 0; index < definition.ColorFrames.ColorsPerFrame; index++)
                {
                    PaletteFxColorKey key = definition.ColorFrames.Color(frame, index);
                    if (!(previousVersionFallback ?? throw new InvalidDataException("A previous-version room palette-FX override requires the current stock presentation.")).TryReadColor(key, out RgbColor stockColor))
                        throw new InvalidDataException(
                            $"Current stock is missing {definition.Suit} heat color {key}.");
                    colors.Add(key, stockColor);
                }
            }
            else
                ValidateAndCompile($"Samus {definition.Suit} suit in heat", frames, definition.ColorFrames, colors);
        }
        foreach (NorfairEnvironmentalPaletteFxColorDefinition definition in
                 NorfairEnvironmentalPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase =>
                    document.NorfairForegroundAndHeatPhase,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette4 =>
                    document.NorfairForegroundPalette4,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette5 =>
                    document.NorfairForegroundPalette5,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette6 =>
                    document.NorfairForegroundPalette6,
                _ => throw new InvalidDataException(
                    $"Unsupported Norfair palette owner {definition.Owner}."),
            };
            ValidateAndCompile($"Norfair {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        foreach (MaridiaEnvironmentalPaletteFxColorDefinition definition in
                 MaridiaEnvironmentalPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                MaridiaEnvironmentalPaletteOwner.SandPits => document.MaridiaSandPits,
                MaridiaEnvironmentalPaletteOwner.SandFalls => document.MaridiaSandFalls,
                MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls =>
                    document.MaridiaBackgroundWaterfalls,
                _ => throw new InvalidDataException(
                    $"Unsupported Maridia palette owner {definition.Owner}."),
            };
            ValidateAndCompile($"Maridia {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("Wrecked Ship green lights", document.WreckedShipGreenLights, WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("Red Brinstar background glow", document.RedBrinstarBackgroundGlow, RedBrinstarGlowPaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("Tourian glow", document.TourianGlow, TourianGlowPaletteFxColorDefinitions.ColorFrames, colors);
        foreach (BrinstarBlueSporePaletteFxColorDefinition definition in
                 BrinstarBlueSporePaletteFxColorDefinitions.All)
        {
            ValidateAndCompile($"Brinstar blue spores ({definition.Owner})", document.BrinstarBlueSpores, definition.ColorFrames, colors);
        }
        foreach (TorizoBellyPaletteFxColorDefinition definition in
                 TorizoBellyPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TorizoBellyPaletteOwner.BombTorizo => document.BombTorizoBelly,
                TorizoBellyPaletteOwner.GoldenTorizo => document.GoldenTorizoBelly,
                _ => throw new InvalidDataException(
                    $"Unsupported Torizo belly owner {definition.Owner}."),
            };
            ValidateAndCompile($"{definition.Owner} belly", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("Tourian statue grey-out", document.TourianStatueGrey, TourianStatueGreyPaletteFxColorDefinitions.ColorFrames, colors);
        foreach (CrateriaLightningPaletteFxColorDefinition definition in
                 CrateriaLightningPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                CrateriaLightningPaletteOwner.SurfaceLightning =>
                    document.CrateriaSurfaceLightning,
                CrateriaLightningPaletteOwner.UnusedDarkLightning =>
                    document.CrateriaUnusedDarkLightning,
                _ => throw new InvalidDataException(
                    $"Unsupported Crateria lightning owner {definition.Owner}."),
            };
            ValidateAndCompile($"Crateria {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("Ceres gunship-engine lights", document.CeresGunshipEngineLights, CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineFrames, colors);
        ValidateAndCompile("Ceres navigation lights", document.CeresNavigationLights, CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsFrames, colors);
        foreach (PlanetZebesTextPaletteFxColorDefinition definition in
                 PlanetZebesTextPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                PlanetZebesTextPaletteFxProgramOwner.FadeIn =>
                    document.PlanetZebesTextFadeIn,
                PlanetZebesTextPaletteFxProgramOwner.FadeOut =>
                    document.PlanetZebesTextFadeOut,
                _ => throw new InvalidDataException(
                    $"Unsupported PLANET ZEBES text-fade owner {definition.Owner}."),
            };
            ValidateAndCompile($"PLANET ZEBES {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        foreach (CinematicGlowPaletteFxColorDefinition definition in
                 CinematicGlowPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights =>
                    document.OldMotherBrainBackgroundLights,
                CinematicGlowPaletteFxProgramOwner.GunshipGlow =>
                    document.CinematicGunshipGlow,
                _ => throw new InvalidDataException(
                    $"Unsupported cinematic glow owner {definition.Owner}."),
            };
            ValidateAndCompile($"Cinematic glow {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("exploding Zebes fade", document.ExplodingZebesFade, ExplodingZebesFadePaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("unused cinematic fade", document.UnusedCinematicFade, UnusedCinematicFadePaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("title-logo fade", document.TitleLogoFade, TitleLogoFadePaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("Nintendo shared fade", document.NintendoSharedFade, NintendoLogoFadePaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("Zebes explosion foreground", document.ZebesExplosionForeground, ZebesExplosionForegroundPaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("Zebes explosion finale", document.ZebesExplosionFinale, ZebesExplosionFinalePaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("Zebes explosion shared whiteout", document.ZebesExplosionWhiteout, ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames, colors);
        foreach (ZebesExplosionAmbientPaletteFxColorDefinition definition in
                 ZebesExplosionAmbientPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow =>
                    document.ZebesExplosionAfterglow,
                ZebesExplosionAmbientPaletteFxProgramOwner.Lava =>
                    document.ZebesExplosionLava,
                _ => throw new InvalidDataException(
                    $"Unsupported Zebes explosion ambient owner {definition.Owner}."),
            };
            ValidateAndCompile($"Zebes explosion {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        foreach (ZebesExplosionLayerFadePaletteFxColorDefinition definition in
                 ZebesExplosionLayerFadePaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                ZebesExplosionLayerFadePaletteFxProgramOwner.Crust =>
                    document.ZebesExplosionCrust,
                ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds =>
                    document.ZebesExplosionGreyClouds,
                _ => throw new InvalidDataException(
                    $"Unsupported Zebes explosion layer owner {definition.Owner}."),
            };
            ValidateAndCompile($"Zebes explosion {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("Zebes explosion gunship", document.ZebesExplosionGunship, ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames, colors);
        foreach (SamusLoadingSuitPaletteFxColorDefinition definition in
                 SamusLoadingSuitPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                SamusLoadingSuitPaletteFxProgramOwner.PowerSuit =>
                    document.SamusLoadingPowerSuit,
                SamusLoadingSuitPaletteFxProgramOwner.VariaSuit =>
                    document.SamusLoadingVariaSuit,
                SamusLoadingSuitPaletteFxProgramOwner.GravitySuit =>
                    document.SamusLoadingGravitySuit,
                _ => throw new InvalidDataException(
                    $"Unsupported Samus loading-suit owner {definition.Owner}."),
            };
            ValidateAndCompile($"Samus loading {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("post-credits icon glare", document.PostCreditsIconGlare, PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames, colors);
        foreach (TourianEscapeRedFlashPaletteFxColorDefinition definition in
                 TourianEscapeRedFlashPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TourianEscapeRedFlashPaletteOwner.Shutter => document.TourianEscapeShutter,
                TourianEscapeRedFlashPaletteOwner.Background =>
                    document.TourianEscapeBackground,
                _ => throw new InvalidDataException(
                    $"Unsupported Tourian escape red-flash owner {definition.Owner}."),
            };
            ValidateAndCompile($"Tourian escape {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("Tourian escape shared red flash", document.TourianEscapeSharedRedFlash, TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorFrames, colors);
        ValidateAndCompile("old Tourian escape red flash", document.OldTourianEscapeRedFlash, OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorFrames, colors);
        foreach (OldTourianEscapeAccentPaletteFxColorDefinition definition in
                 OldTourianEscapeAccentPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                OldTourianEscapeAccentPaletteOwner.OrangeRailings =>
                    document.OldTourianEscapeOrangeRailings,
                OldTourianEscapeAccentPaletteOwner.YellowPanels =>
                    document.OldTourianEscapeYellowPanels,
                _ => throw new InvalidDataException(
                    $"Unsupported old Tourian escape accent owner {definition.Owner}."),
            };
            ValidateAndCompile($"old Tourian escape {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("upper Crateria escape red flash", document.UpperCrateriaEscapeRedFlash, UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorFrames, colors);
        foreach (CrateriaEscapeLightningPaletteFxColorDefinition definition in
                 CrateriaEscapeLightningPaletteFxColorDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                CrateriaEscapeLightningPaletteOwner.YellowLightning =>
                    document.CrateriaEscapeYellowLightning,
                CrateriaEscapeLightningPaletteOwner.CreBlockPixel =>
                    document.CrateriaEscapeCreBlockPixel,
                _ => throw new InvalidDataException(
                    $"Unsupported Crateria escape lightning owner {definition.Owner}."),
            };
            ValidateAndCompile($"Crateria escape {definition.Owner}", frames, definition.ColorFrames, colors);
        }
        ValidateAndCompile("Crateria and Brinstar beacon flash", document.BeaconFlashing, BeaconPaletteFxColorDefinitions.ColorFrames, colors);

        return RoomPaletteFxPresentation.Create(colors);
    }

    /// <summary>Serializes a current-version presentation document and validates it before writing any bytes to the destination.</summary>
    /// <param name="json">The stream that receives the validated UTF-8 JSON document.</param>
    /// <param name="document">The complete editable color document; animation timing and destinations remain defined by engine mechanics.</param>
    /// <exception cref="InvalidDataException">The document fails the current schema or palette-row validation.</exception>
    public static void Write(Stream json, RoomPaletteFxPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document,
            ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Validates one color family's frame geometry and stores each RGB5 sample by its record color.</summary>
    private static void ValidateAndCompile(
        string owner,
        PaletteRgb5[][]? frames,
        PaletteFxColorFrames colorFrames,
        Dictionary<PaletteFxColorKey, RgbColor> destination)
    {
        int frameCount = colorFrames.FrameCount;
        int colorCount = colorFrames.ColorsPerFrame;
        if (frames is null || frames.Length != frameCount)
        {
            throw new InvalidDataException(
                $"Room palette-FX {owner} requires exactly {frameCount} frames.");
        }

        for (int frame = 0; frame < frames.Length; frame++)
        {
            PaletteRgb5[]? frameColors = frames[frame];
            if (frameColors is null || frameColors.Length != colorCount)
            {
                throw new InvalidDataException(
                    $"Room palette-FX {owner} frame {frame} requires exactly " +
                    $"{colorCount} colors.");
            }

            for (int index = 0; index < frameColors.Length; index++)
            {
                PaletteRgb5? color = frameColors[index];
                if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                    (uint)color.Blue > 31)
                {
                    throw new InvalidDataException(
                        $"Room palette-FX {owner} frame {frame} color {index} " +
                        "requires RGB components from 0 to 31.");
                }

                destination.Add(
                    colorFrames.Color(frame, index),
                    color.ToRgbColor());
            }
        }
    }
}
