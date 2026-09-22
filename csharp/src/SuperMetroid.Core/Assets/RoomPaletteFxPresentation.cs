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

    private RoomPaletteFxPresentation(Dictionary<ushort, ushort> colors) =>
        this.colors = colors;

    /// <inheritdoc />
    public bool TryReadColor(ushort pointer, out ushort color) =>
        colors.TryGetValue(pointer, out color);

    public static RoomPaletteFxPresentation Load(Stream json)
    {
        RoomPaletteFxPresentationDocument document;
        try
        {
            document = JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(
                json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Room palette-FX presentation is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room palette-FX presentation JSON.", error);
        }

        if (document.Version != RoomPaletteFxPresentationFormat.Version)
        {
            throw new InvalidDataException(
                $"Room palette-FX presentation requires version " +
                $"{RoomPaletteFxPresentationFormat.Version}.");
        }

        var colors = new Dictionary<ushort, ushort>();
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
}

public static class RoomPaletteFxPresentationFormat
{
    public const string FileName = "room-palette-effects.json";
    public const int Version = 14;
}
