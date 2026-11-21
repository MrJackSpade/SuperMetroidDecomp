using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Every colour program of the room palette-effect document, each listed once, in the document's order.</summary>
internal static class RoomPaletteFxPrograms
{
    /// <summary>The programs, in the order the document stores them.</summary>
    internal static IReadOnlyList<RoomPaletteFxProgram> All { get; } =
    [
        new("samusHeatPowerSuit", [new("Samus Power suit in heat", PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Power).ColorFrames)]),
        new("samusHeatVariaSuit", [new("Samus Varia suit in heat", PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Varia).ColorFrames)]),
        new("samusHeatGravitySuit", [new("Samus Gravity suit in heat", PaletteFxHeatPulseColorDefinitions.All.Single(item => item.Suit == ColourEffectHeatSuit.Gravity).ColorFrames)]),
        new("norfairForegroundAndHeatPhase", [new("Norfair foreground and heat phase", NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase).ColorFrames)]),
        new("norfairGreenForeground", [new("Norfair green foreground", NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.GreenForeground).ColorFrames)]),
        new("norfairPurpleForeground", [new("Norfair purple foreground", NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.PurpleForeground).ColorFrames)]),
        new("norfairDarkForeground", [new("Norfair dark foreground", NorfairEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == NorfairEnvironmentalPaletteOwner.DarkForeground).ColorFrames)]),
        new("maridiaSandPits", [new("Maridia sand pits", MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.SandPits).ColorFrames)]),
        new("maridiaSandFalls", [new("Maridia sand falls", MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.SandFalls).ColorFrames)]),
        new("maridiaBackgroundWaterfalls", [new("Maridia background waterfalls", MaridiaEnvironmentalPaletteFxColorDefinitions.All.Single(item => item.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls).ColorFrames)]),
        new("wreckedShipGreenLights", [new("Wrecked Ship green lights", WreckedShipGreenLightPaletteFxColorDefinitions.ColorFrames)]),
        new("redBrinstarBackgroundGlow", [new("Red Brinstar background glow", RedBrinstarGlowPaletteFxColorDefinitions.ColorFrames)]),
        new("tourianGlow", [new("Tourian glow", TourianGlowPaletteFxColorDefinitions.ColorFrames)]),
        new("brinstarBlueSpores", [.. BrinstarBlueSporePaletteFxColorDefinitions.All.Select(definition => new RoomPaletteFxRun($"Brinstar blue spores ({definition.Owner})", definition.ColorFrames))]),
        new("bombTorizoBelly", [new("Bomb Torizo belly", TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.BombTorizo).ColorFrames)]),
        new("goldenTorizoBelly", [new("Golden Torizo belly", TorizoBellyPaletteFxColorDefinitions.All.Single(item => item.Owner == TorizoBellyPaletteOwner.GoldenTorizo).ColorFrames)]),
        new("tourianStatueGrey", [new("Tourian statue grey-out", TourianStatueGreyPaletteFxColorDefinitions.ColorFrames)]),
        new("crateriaSurfaceLightning", [new("Crateria surface lightning", CrateriaLightningPaletteFxColorDefinitions.Surface.ColorFrames)]),
        new("ceresGunshipEngineLights", [new("Ceres gunship-engine lights", CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineFrames)]),
        new("ceresNavigationLights", [new("Ceres navigation lights", CeresCinematicLightPaletteFxColorDefinitions.NavigationLightsFrames)]),
        new("planetZebesTextFadeIn", [new("PLANET ZEBES fade-in", PlanetZebesTextPaletteFxColorDefinitions.All.Single(item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeIn).ColorFrames)]),
        new("planetZebesTextFadeOut", [new("PLANET ZEBES fade-out", PlanetZebesTextPaletteFxColorDefinitions.All.Single(item => item.Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut).ColorFrames)]),
        new("oldMotherBrainBackgroundLights", [new("cinematic old Mother Brain background lights", CinematicGlowPaletteFxColorDefinitions.All.Single(item => item.Owner == CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights).ColorFrames)]),
        new("cinematicGunshipGlow", [new("cinematic gunship glow", CinematicGlowPaletteFxColorDefinitions.All.Single(item => item.Owner == CinematicGlowPaletteFxProgramOwner.GunshipGlow).ColorFrames)]),
        new("explodingZebesFade", [new("exploding Zebes fade", ExplodingZebesFadePaletteFxColorDefinitions.ColorFrames)]),
        new("titleLogoFade", [new("title-logo fade", TitleLogoFadePaletteFxColorDefinitions.ColorFrames)]),
        new("nintendoSharedFade", [new("Nintendo shared fade", NintendoLogoFadePaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionForeground", [new("Zebes explosion foreground", ZebesExplosionForegroundPaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionFinale", [new("Zebes explosion finale", ZebesExplosionFinalePaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionWhiteout", [new("Zebes explosion shared whiteout", ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorFrames)]),
        new("zebesExplosionAfterglow", [new("Zebes explosion planet afterglow", ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow).ColorFrames)]),
        new("zebesExplosionLava", [new("Zebes explosion lava", ZebesExplosionAmbientPaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionAmbientPaletteFxProgramOwner.Lava).ColorFrames)]),
        new("zebesExplosionCrust", [new("Zebes explosion crust", ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.Crust).ColorFrames)]),
        new("zebesExplosionGreyClouds", [new("Zebes explosion grey clouds", ZebesExplosionLayerFadePaletteFxColorDefinitions.All.Single(item => item.Owner == ZebesExplosionLayerFadePaletteFxProgramOwner.GreyClouds).ColorFrames)]),
        new("zebesExplosionGunship", [new("Zebes explosion gunship", ZebesExplosionGunshipPaletteFxColorDefinitions.ColorFrames)]),
        new("samusLoadingPowerSuit", [new("Samus loading Power suit", SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.PowerSuit).ColorFrames)]),
        new("samusLoadingVariaSuit", [new("Samus loading Varia suit", SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.VariaSuit).ColorFrames)]),
        new("samusLoadingGravitySuit", [new("Samus loading Gravity suit", SamusLoadingSuitPaletteFxColorDefinitions.All.Single(item => item.Owner == SamusLoadingSuitPaletteFxProgramOwner.GravitySuit).ColorFrames)]),
        new("postCreditsIconGlare", [new("post-credits icon glare", PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames)]),
        new("tourianEscapeShutter", [new("Tourian escape shutter", TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(item => item.Owner == TourianEscapeRedFlashPaletteOwner.Shutter).ColorFrames)]),
        new("tourianEscapeBackground", [new("Tourian escape background", TourianEscapeRedFlashPaletteFxColorDefinitions.All.Single(item => item.Owner == TourianEscapeRedFlashPaletteOwner.Background).ColorFrames)]),
        new("tourianEscapeSharedRedFlash", [new("Tourian escape shared red flash", TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorFrames)]),
        new("oldTourianEscapeRedFlash", [new("old Tourian escape red flash", OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorFrames)]),
        new("oldTourianEscapeOrangeRailings", [new("old Tourian escape orange railings", OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(item => item.Owner == OldTourianEscapeAccentPaletteOwner.OrangeRailings).ColorFrames)]),
        new("oldTourianEscapeYellowPanels", [new("old Tourian escape yellow panels", OldTourianEscapeAccentPaletteFxColorDefinitions.All.Single(item => item.Owner == OldTourianEscapeAccentPaletteOwner.YellowPanels).ColorFrames)]),
        new("upperCrateriaEscapeRedFlash", [new("upper Crateria escape red flash", UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorFrames)]),
        new("crateriaEscapeYellowLightning", [new("Crateria escape yellow lightning", CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(item => item.Owner == CrateriaEscapeLightningPaletteOwner.YellowLightning).ColorFrames)]),
        new("parlorEscapeSky", [new("Crateria escape Parlor sky", CrateriaEscapeLightningPaletteFxColorDefinitions.All.Single(item => item.Owner == CrateriaEscapeLightningPaletteOwner.ParlorSky).ColorFrames)]),
        new("beaconFlashing", [new("Crateria and Brinstar beacon flash", BeaconPaletteFxColorDefinitions.ColorFrames)]),
    ];
}
