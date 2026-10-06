using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable colors for room and cinematic palette animations. Native timing, destinations,
/// instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    private readonly Dictionary<ushort, ushort> colors;

    private readonly HeatPaletteInputView heatInputs;
    private readonly LoadingPaletteInputView loadingInputs;
    private readonly EndingGunshipPaletteInputView gunshipInputs;

    private RoomPaletteFxPresentation(Dictionary<ushort, ushort> colors)
    {
        this.colors = colors;
        heatInputs = new HeatPaletteInputView(colors);
        loadingInputs = new LoadingPaletteInputView(colors);
        gunshipInputs = new EndingGunshipPaletteInputView(colors);
    }

    /// <inheritdoc />
    public bool TryReadColor(ushort pointer, out ushort color) =>
        gunshipInputs.TryGetValue(pointer, out color) ||
        PlanetZebesTextColorDefinitions.TryCalculate(pointer, colors, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedColor(pointer, out color) ||
        CrateriaLightningColorDefinitions.TryCalculatedDarkColor(pointer, colors, out color) ||
        TourianStatueGreyColorDefinitions.TryCalculatedColor(pointer, colors, out color) ||
        LoadingPaletteColorDefinitions.TryReadColor(pointer, loadingInputs, out color) ||
        LogoGlarePaletteColorDefinitions.TryCalculatedColor(pointer, colors, out color) ||
        EndingGunshipPaletteColorDefinitions.TryCalculatedColor(pointer, gunshipInputs, out color) ||
        (HeatPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
         (heatInputs.TryGetValue(canonical, out color) ||
          HeatPaletteColorDefinitions.TryCalculatedColor(canonical, heatInputs, out color)));

    /// <summary>
    /// Installed identities, including shared-color aliases, for the development
    /// dependency auditor. This exposes keys only, not cartridge bytes or colors.
    /// </summary>
    internal IEnumerable<ushort> ColorPointers
    {
        get
        {
            foreach (ushort pointer in colors.Keys)
                if (!PlanetZebesTextColorDefinitions.TryCoordinates(pointer, out _, out _, out _) &&
                    !CrateriaLightningColorDefinitions.TryCoordinates(pointer, out _, out _) &&
                    !CrateriaLightningColorDefinitions.TryDarkCoordinates(pointer, out _, out _) &&
                    !HeatPaletteColorDefinitions.TryCanonicalPointer(pointer, out _) &&
                    !LoadingPaletteColorDefinitions.TryCanonicalPointer(pointer, out _) &&
                    !LogoGlarePaletteColorDefinitions.TryCoordinates(pointer, out _, out _) &&
                    !EndingGunshipPaletteColorDefinitions.TryCoordinates(pointer, out _, out _) &&
                    !TourianStatueGreyColorDefinitions.TryCoordinates(pointer, out _, out _)) yield return pointer;
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

    public static RoomPaletteFxPresentation Load(Stream json,
        RoomPaletteFxPresentation? previousVersionFallback = null)
    {
        RoomPaletteFxPresentationDocument document = JsonAssetDocument.Read<RoomPaletteFxPresentationDocument>(
            json, MapPresentationFormat.JsonOptions, "room palette-FX presentation");

        if (document.Version != RoomPaletteFxPresentationFormat.Version &&
            !(document.Version == RoomPaletteFxPresentationFormat.PreviousVersion &&
              previousVersionFallback is not null))
        {
            throw new InvalidDataException(
                $"Room palette-FX presentation requires version " +
                $"{RoomPaletteFxPresentationFormat.Version}.");
        }

        var colors = new Dictionary<ushort, ushort>();
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
            ushort ColorPointer(int frame, int index) => unchecked((ushort)(
                definition.Frames[frame].FirstColorPointer + index * sizeof(ushort)));
            if (document.Version == RoomPaletteFxPresentationFormat.PreviousVersion)
            {
                // A v17 player override predates these three color families. Keep
                // all its existing edits and inherit only the newly extracted
                // Samus-in-heat rows from the verified current stock catalog.
                for (int frame = 0; frame < definition.Frames.Count; frame++)
                for (int index = 0; index < PaletteFxHeatProgramDefinition.ColorsPerFrame; index++)
                {
                    ushort pointer = ColorPointer(frame, index);
                    if (!previousVersionFallback!.TryReadColor(pointer, out ushort stockColor))
                        throw new InvalidDataException(
                            $"Current stock is missing {definition.Suit} heat color $8D:{pointer:X4}.");
                    colors.Add(pointer, stockColor);
                }
            }
            else
                ValidateAndCompile($"Samus {definition.Suit} suit in heat", frames,
                    definition.Frames.Count, PaletteFxHeatProgramDefinition.ColorsPerFrame,
                    ColorPointer, colors);
        }
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                 NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Norfair {definition.Owner}",
                frames,
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount,
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                 MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Maridia {definition.Owner}",
                frames,
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Wrecked Ship green lights",
            document.WreckedShipGreenLights,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Red Brinstar background glow",
            document.RedBrinstarBackgroundGlow,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Tourian glow",
            document.TourianGlow,
            TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in
                 BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All)
        {
            ValidateAndCompile(
                $"Brinstar blue spores ({definition.Owner})",
                document.BrinstarBlueSpores,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount,
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (TorizoBellyPaletteFxProgramDefinition definition in
                 TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TorizoBellyPaletteOwner.BombTorizo => document.BombTorizoBelly,
                TorizoBellyPaletteOwner.GoldenTorizo => document.GoldenTorizoBelly,
                _ => throw new InvalidDataException(
                    $"Unsupported Torizo belly owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"{definition.Owner} belly",
                frames,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Tourian statue grey-out",
            document.TourianStatueGrey,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (CrateriaLightningPaletteFxProgramDefinition definition in
                 CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Crateria {definition.Owner}",
                frames,
                definition.Frames.Count,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Ceres gunship-engine lights",
            document.CeresGunshipEngineLights,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineFrameCount,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorsPerFrame,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorPointer,
            colors);
        ValidateAndCompile(
            "Ceres navigation lights",
            document.CeresNavigationLights,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsFrameCount,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsColorsPerFrame,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .NavigationLightsColorPointer,
            colors);
        foreach (PlanetZebesTextPaletteFxProgramDefinition definition in
                 PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"PLANET ZEBES {definition.Owner}",
                frames,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (CinematicGlowPaletteFxProgramDefinition definition in
                 CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Cinematic glow {definition.Owner}",
                frames,
                CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "exploding Zebes fade",
            document.ExplodingZebesFade,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "unused cinematic fade",
            document.UnusedCinematicFade,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "title-logo fade",
            document.TitleLogoFade,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Nintendo shared fade",
            document.NintendoSharedFade,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion foreground",
            document.ZebesExplosionForeground,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion finale",
            document.ZebesExplosionFinale,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "Zebes explosion shared whiteout",
            document.ZebesExplosionWhiteout,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition definition in
                 ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Zebes explosion {definition.Owner}",
                frames,
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition definition in
                 ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Zebes explosion {definition.Owner}",
                frames,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Zebes explosion gunship",
            document.ZebesExplosionGunship,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile(
                $"Samus loading {definition.Owner}",
                frames,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "post-credits icon glare",
            document.PostCreditsIconGlare,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in
                 TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TourianEscapeRedFlashPaletteOwner.Shutter => document.TourianEscapeShutter,
                TourianEscapeRedFlashPaletteOwner.Background =>
                    document.TourianEscapeBackground,
                _ => throw new InvalidDataException(
                    $"Unsupported Tourian escape red-flash owner {definition.Owner}."),
            };
            ValidateAndCompile($"Tourian escape {definition.Owner}", frames,
                TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "Tourian escape shared red flash",
            document.TourianEscapeSharedRedFlash,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        ValidateAndCompile(
            "old Tourian escape red flash",
            document.OldTourianEscapeRedFlash,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in
                 OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile($"old Tourian escape {definition.Owner}", frames,
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount,
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "upper Crateria escape red flash",
            document.UpperCrateriaEscapeRedFlash,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition definition in
                 CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
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
            ValidateAndCompile($"Crateria escape {definition.Owner}", frames,
                CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount,
                definition.ColorsPerFrame, definition.ColorPointer, colors);
        }
        ValidateAndCompile(
            "Crateria and Brinstar beacon flash",
            document.BeaconFlashing,
            BeaconPaletteFxProgramMechanicsDefinitions.FrameCount,
            BeaconPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            BeaconPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);

        // Collapse only values equal to the calculated row alias. Differing player
        // edits keep their original per-frame identity, including edits to base rows.
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (var frame in program.Frames)
        for (int index = 0; index < 15; index++)
        {
            ushort pointer = (ushort)(frame.FirstColorPointer + 2 * index);
            if (HeatPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
                canonical != pointer && colors[pointer] == colors[canonical]) colors.Remove(pointer);
        }
        foreach (var program in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (var frame in program.Frames)
        for (int index = 0; index < 15; index++)
        {
            ushort pointer = (ushort)(frame.FirstColorPointer + 2 * index);
            if (colors.TryGetValue(pointer, out ushort supplied) &&
                HeatPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out ushort calculated) && supplied == calculated)
                colors.Remove(pointer);
        }
        foreach (var program in SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < 9; frame++)
        for (int index = 0; index < 16; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (LoadingPaletteColorDefinitions.TryCanonicalPointer(pointer, out ushort canonical) &&
                canonical != pointer && colors[pointer] == colors[canonical]) colors.Remove(pointer);
        }
        foreach (var program in SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < 9; frame++)
        for (int index = 0; index < 16; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (colors.TryGetValue(pointer, out ushort supplied) &&
                LoadingPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out ushort calculated) && supplied == calculated)
                colors.Remove(pointer);
        }
        // Only matching samples are discarded. If the player edits a base color,
        // unchanged intermediate samples that no longer fit remain explicit values.
        for (int frame = 0; frame < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
            if (LogoGlarePaletteColorDefinitions.TryCalculatedColor(pointer, colors, out ushort calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        for (int frame = 0; frame < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
            if (EndingGunshipPaletteColorDefinitions.TryCalculatedColor(pointer, colors, out ushort calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        // Keep both endpoints and independent sample edits. Only values matching
        // their supplied endpoints can be reconstructed on demand.
        for (int frame = 1; frame < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount - 1; frame++)
        for (int index = 0; index < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, index);
            if (TourianStatueGreyColorDefinitions.TryCalculatedColor(pointer, colors, out ushort calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        var surfaceLightning = CrateriaLightningColorDefinitions.SurfaceProgram;
        for (int frame = 0; frame < surfaceLightning.Frames.Count; frame++)
        for (int index = 0; index < surfaceLightning.ColorsPerFrame; index++)
        {
            ushort pointer = surfaceLightning.ColorPointer(frame, index);
            if (CrateriaLightningColorDefinitions.TryCalculatedColor(pointer, out ushort calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        var darkLightning = CrateriaLightningColorDefinitions.DarkProgram;
        for (int frame = 1; frame < darkLightning.Frames.Count; frame++)
        for (int index = 0; index < darkLightning.ColorsPerFrame; index++)
        {
            ushort pointer = darkLightning.ColorPointer(frame, index);
            if (CrateriaLightningColorDefinitions.TryCalculatedDarkColor(pointer, colors, out ushort calculated) &&
                colors[pointer] == calculated) colors.Remove(pointer);
        }
        foreach (var program in PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
        for (int index = 0; index < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; index++)
        {
            ushort pointer = program.ColorPointer(frame, index);
            if (pointer != PlanetZebesTextColorDefinitions.EndpointPointer(index)
                && PlanetZebesTextColorDefinitions.TryCalculate(pointer, colors, out ushort calculated)
                && colors[pointer] == calculated) colors.Remove(pointer);
        }
        return new RoomPaletteFxPresentation(colors);
    }

    public static void Write(Stream json, RoomPaletteFxPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    private static void ValidateAndCompile(
        string owner,
        PaletteRgb5[][]? frames,
        int frameCount,
        int colorCount,
        Func<int, int, ushort> colorPointer,
        Dictionary<ushort, ushort> destination)
    {
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

                ushort pointer = colorPointer(frame, index);
                destination.Add(
                    pointer,
                    (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
            }
        }
    }
}

public sealed record RoomPaletteFxPresentationDocument
{
    public required int Version { get; init; }
    /// <summary>Sixteen native phases of Samus's Power Suit palette in heat.</summary>
    public PaletteRgb5[][]? SamusHeatPowerSuit { get; init; }
    /// <summary>Sixteen native phases of Samus's Varia Suit palette in heat.</summary>
    public PaletteRgb5[][]? SamusHeatVariaSuit { get; init; }
    /// <summary>Sixteen native phases of Samus's Gravity Suit palette in heat.</summary>
    public PaletteRgb5[][]? SamusHeatGravitySuit { get; init; }
    public required PaletteRgb5[][] NorfairForegroundAndHeatPhase { get; init; }
    public required PaletteRgb5[][] NorfairForegroundPalette4 { get; init; }
    public required PaletteRgb5[][] NorfairForegroundPalette5 { get; init; }
    public required PaletteRgb5[][] NorfairForegroundPalette6 { get; init; }
    public required PaletteRgb5[][] MaridiaSandPits { get; init; }
    public required PaletteRgb5[][] MaridiaSandFalls { get; init; }
    public required PaletteRgb5[][] MaridiaBackgroundWaterfalls { get; init; }
    public required PaletteRgb5[][] WreckedShipGreenLights { get; init; }
    public required PaletteRgb5[][] RedBrinstarBackgroundGlow { get; init; }
    public required PaletteRgb5[][] TourianGlow { get; init; }
    public required PaletteRgb5[][] BrinstarBlueSpores { get; init; }
    public required PaletteRgb5[][] BombTorizoBelly { get; init; }
    public required PaletteRgb5[][] GoldenTorizoBelly { get; init; }
    public required PaletteRgb5[][] TourianStatueGrey { get; init; }
    public required PaletteRgb5[][] CrateriaSurfaceLightning { get; init; }
    public required PaletteRgb5[][] CrateriaUnusedDarkLightning { get; init; }
    public required PaletteRgb5[][] CeresGunshipEngineLights { get; init; }
    public required PaletteRgb5[][] CeresNavigationLights { get; init; }
    public required PaletteRgb5[][] PlanetZebesTextFadeIn { get; init; }
    public required PaletteRgb5[][] PlanetZebesTextFadeOut { get; init; }
    public required PaletteRgb5[][] OldMotherBrainBackgroundLights { get; init; }
    public required PaletteRgb5[][] CinematicGunshipGlow { get; init; }
    public required PaletteRgb5[][] ExplodingZebesFade { get; init; }
    public required PaletteRgb5[][] UnusedCinematicFade { get; init; }
    public required PaletteRgb5[][] TitleLogoFade { get; init; }
    public required PaletteRgb5[][] NintendoSharedFade { get; init; }
    public required PaletteRgb5[][] ZebesExplosionForeground { get; init; }
    public required PaletteRgb5[][] ZebesExplosionFinale { get; init; }
    public required PaletteRgb5[][] ZebesExplosionWhiteout { get; init; }
    public required PaletteRgb5[][] ZebesExplosionAfterglow { get; init; }
    public required PaletteRgb5[][] ZebesExplosionLava { get; init; }
    public required PaletteRgb5[][] ZebesExplosionCrust { get; init; }
    public required PaletteRgb5[][] ZebesExplosionGreyClouds { get; init; }
    public required PaletteRgb5[][] ZebesExplosionGunship { get; init; }
    public required PaletteRgb5[][] SamusLoadingPowerSuit { get; init; }
    public required PaletteRgb5[][] SamusLoadingVariaSuit { get; init; }
    public required PaletteRgb5[][] SamusLoadingGravitySuit { get; init; }
    public required PaletteRgb5[][] PostCreditsIconGlare { get; init; }
    public required PaletteRgb5[][] TourianEscapeShutter { get; init; }
    public required PaletteRgb5[][] TourianEscapeBackground { get; init; }
    public required PaletteRgb5[][] TourianEscapeSharedRedFlash { get; init; }
    public required PaletteRgb5[][] OldTourianEscapeRedFlash { get; init; }
    public required PaletteRgb5[][] OldTourianEscapeOrangeRailings { get; init; }
    public required PaletteRgb5[][] OldTourianEscapeYellowPanels { get; init; }
    public required PaletteRgb5[][] UpperCrateriaEscapeRedFlash { get; init; }
    public required PaletteRgb5[][] CrateriaEscapeYellowLightning { get; init; }
    public required PaletteRgb5[][] CrateriaEscapeCreBlockPixel { get; init; }
    public required PaletteRgb5[][] BeaconFlashing { get; init; }
}

public static class RoomPaletteFxPresentationFormat
{
    public const string FileName = "room-palette-effects.json";
    public const int PreviousVersion = 17;
    public const int Version = 18;
}
