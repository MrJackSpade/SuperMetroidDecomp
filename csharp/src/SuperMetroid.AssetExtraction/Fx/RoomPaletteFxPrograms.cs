using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// One colour program of the installed room palette-effect document (#2557): the key its colours are stored under, how a
/// failure names it, and the record colours it holds, each record run validated against the document's frames. The
/// Brinstar blue spores' ordinary and Spore Spawn programs share one entry.
/// </summary>
/// <param name="Key">The key the program's frames are stored under in the document.</param>
/// <param name="Description">How a failure names the program.</param>
/// <param name="Frames">The record runs the stored frames give colours to; extraction reads the first.</param>
internal sealed record RoomPaletteFxProgram(string Key, string Description, IReadOnlyList<PaletteFxColorFrames> Frames);

/// <summary>Every colour program of the room palette-effect document, each listed once, in the document's order.</summary>
internal static class RoomPaletteFxPrograms
{
    /// <summary>The programs, in the order the document stores them.</summary>
    internal static IReadOnlyList<RoomPaletteFxProgram> All { get; } =
    [
        new("samusHeatPowerSuit", "Samus Power suit in heat", [PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Power).ColorFrames]),
        new("samusHeatVariaSuit", "Samus Varia suit in heat", [PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Varia).ColorFrames]),
        new("samusHeatGravitySuit", "Samus Gravity suit in heat", [PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Gravity).ColorFrames]),
        new("norfairForegroundAndHeatPhase", "Norfair foreground and heat phase", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase).ColorFrames]),
        new("norfairGreenForeground", "Norfair green foreground", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.GreenForeground).ColorFrames]),
        new("norfairPurpleForeground", "Norfair purple foreground", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.PurpleForeground).ColorFrames]),
        new("norfairDarkForeground", "Norfair dark foreground", [NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.DarkForeground).ColorFrames]),
        new("maridiaSandPits", "Maridia sand pits", [MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.SandPits).ColorFrames]),
        new("maridiaSandFalls", "Maridia sand falls", [MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.SandFalls).ColorFrames]),
        new("maridiaBackgroundWaterfalls", "Maridia background waterfalls", [MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls).ColorFrames]),
        new("wreckedShipGreenLights", "Wrecked Ship green lights", [WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames]),
        new("redBrinstarBackgroundGlow", "Red Brinstar background glow", [RedBrinstarGlowPaletteFxColorDefinitions.ColorFrames]),
        new("tourianGlow", "Tourian glow", [TourianGlowPaletteFxColorDefinitions.ColorFrames]),
        new("brinstarBlueSpores", "Brinstar blue spores", [.. BrinstarBlueSporePaletteFxColorDefinitions.All.Select(definition => definition.ColorFrames)]),
        new("bombTorizoBelly", "Bomb Torizo belly", [TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.BombTorizo).ColorFrames]),
        new("goldenTorizoBelly", "Golden Torizo belly", [TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.GoldenTorizo).ColorFrames]),
        new("tourianStatueGrey", "Tourian statue grey-out", [TourianStatueGreyPaletteFxColorDefinitions.ColorFrames]),
        new("crateriaSurfaceLightning", "Crateria surface lightning", [CrateriaLightningPaletteFxColorDefinitions.Surface.ColorFrames]),
        new("ceresGunshipEngineLights", "Ceres gunship-engine lights", [CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineFrames]),
        new("ceresNavigationLights", "Ceres navigation lights", [CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsFrames]),
        new("planetZebesTextFadeIn", "PLANET ZEBES fade-in", [PlanetZebesTextPaletteFxColorDefinitions.All.Single(item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn).ColorFrames]),
        new("planetZebesTextFadeOut", "PLANET ZEBES fade-out", [PlanetZebesTextPaletteFxColorDefinitions.All.Single(item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut).ColorFrames]),
        new("oldMotherBrainBackgroundLights", "cinematic old Mother Brain background lights", [CinematicGlowPaletteFxColorDefinitions.All.Single(item => item.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights).ColorFrames]),
        new("cinematicGunshipGlow", "cinematic gunship glow", [CinematicGlowPaletteFxColorDefinitions.All.Single(item => item.Owner == CinematicGlowPaletteFxProgramOwner.GunshipGlow).ColorFrames]),
        new("explodingZebesFade", "exploding Zebes fade", [ExplodingZebesFadePaletteFxColorDefinitions.ColorFrames]),
        new("titleLogoFade", "title-logo fade", [TitleLogoFadePaletteFxColorDefinitions.ColorFrames]),
        new("nintendoSharedFade", "Nintendo shared fade", [NintendoLogoFadePaletteFxColorDefinitions.ColorFrames]),
        new("zebesExplosionForeground", "Zebes explosion foreground", [ZebesExplosionForegroundPaletteFxColorDefinitions.ColorFrames]),
        new("zebesExplosionFinale", "Zebes explosion finale", [ZebesExplosionFinalePaletteFxColorDefinitions.ColorFrames]),
        new("zebesExplosionWhiteout", "Zebes explosion shared whiteout", [ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames]),
        new("zebesExplosionAfterglow", "Zebes explosion planet afterglow", [ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow).ColorFrames]),
        new("zebesExplosionLava", "Zebes explosion lava", [ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.Lava).ColorFrames]),
        new("zebesExplosionCrust", "Zebes explosion crust", [ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.Crust).ColorFrames]),
        new("zebesExplosionGreyClouds", "Zebes explosion grey clouds", [ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds).ColorFrames]),
        new("zebesExplosionGunship", "Zebes explosion gunship", [ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames]),
        new("samusLoadingPowerSuit", "Samus loading Power suit", [SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.PowerSuit).ColorFrames]),
        new("samusLoadingVariaSuit", "Samus loading Varia suit", [SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.VariaSuit).ColorFrames]),
        new("samusLoadingGravitySuit", "Samus loading Gravity suit", [SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.GravitySuit).ColorFrames]),
        new("postCreditsIconGlare", "post-credits icon glare", [PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames]),
        new("tourianEscapeShutter", "Tourian escape shutter", [TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(item => item.Owner == TourianEscapeRedFlashPaletteOwner.Shutter).ColorFrames]),
        new("tourianEscapeBackground", "Tourian escape background", [TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(item => item.Owner == TourianEscapeRedFlashPaletteOwner.Background).ColorFrames]),
        new("tourianEscapeSharedRedFlash", "Tourian escape shared red flash", [TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorFrames]),
        new("oldTourianEscapeRedFlash", "old Tourian escape red flash", [OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorFrames]),
        new("oldTourianEscapeOrangeRailings", "old Tourian escape orange railings", [OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(item => item.Owner == OldTourianEscapeAccentPaletteOwner.OrangeRailings).ColorFrames]),
        new("oldTourianEscapeYellowPanels", "old Tourian escape yellow panels", [OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(item => item.Owner == OldTourianEscapeAccentPaletteOwner.YellowPanels).ColorFrames]),
        new("upperCrateriaEscapeRedFlash", "upper Crateria escape red flash", [UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorFrames]),
        new("crateriaEscapeYellowLightning", "Crateria escape yellow lightning", [CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(item => item.Owner == CrateriaEscapeLightningPaletteOwner.YellowLightning).ColorFrames]),
        new("parlorEscapeSky", "Crateria escape Parlor sky", [CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(item => item.Owner == CrateriaEscapeLightningPaletteOwner.ParlorSky).ColorFrames]),
        new("beaconFlashing", "Crateria and Brinstar beacon flash", [BeaconPaletteFxColorDefinitions.ColorFrames]),
    ];
}
