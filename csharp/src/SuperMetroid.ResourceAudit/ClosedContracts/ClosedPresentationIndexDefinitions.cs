using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Finite index domains from the source-reviewed loader/selector contracts.</summary>
internal static class ClosedPresentationIndexDefinitions
{
    /// <summary>Returns a source-reviewed finite domain for a provider parameter, or null when no closed numeric proof exists.</summary>
    internal static ProviderIndexDomain? Get(string type, string method, string parameter) => new ClosedPresentationIndexGetCase(type, method, parameter) switch
    {
        ("GameplayHudPresentation", "EnergyTank", "tank") => new(0, GameplayHudDefinitions.EnergyTankCount),
        ("GameplayHudPresentation", "MinimapCell", "output.X") => new(0, GameplayHudPresentation.MinimapSize.X),
        ("GameplayHudPresentation", "MinimapCell", "output.Y") => new(0, GameplayHudPresentation.MinimapSize.Y),
        ("FileSelectPresentation", "Slot" or "DrawHelmet", "slot") => new(0, 3),
        ("FileSelectPresentation", "DrawCursor", "frame") => new(0, 4),
        ("FileSelectPresentation", "DrawHelmet", "frame") => new(0, 8),
        ("FileSelectPresentation", "CursorPosition", "selected") => new(0, 6),
        ("GameOptionsPresentation", "DrawCursor", "frame") => new(0, 4),
        ("GameOverPresentation", "DrawBaby", "frame") => new(0, 3),
        ("GameOverPresentation", "DrawCursor", "frame") => new(0, 4),
        ("PauseReserveUiPresentation", "ApplyDigit", "position") => new(0, PauseReserveUiDefinitions.SupplyDigitPlaces),
        ("PauseReserveUiPresentation", "ApplyDigit", "value") => new(0, PauseReserveUiDefinitions.DigitCount),
        ("InstalledBeamPalettes", "Colours", "selection") => new(0, BeamPaletteFile.SelectionCount),
        ("PhantoonColorCatalog", "ResolveHealth", "band") => new(0, PhantoonColorDefinitions.HealthBandCount),
        ("PhantoonColorCatalog", "ResolveHealth", "color") => new(0, PhantoonColorFormat.HealthBandColorCount),
        ("PhantoonColorCatalog", "ResolveFadeOut", "color") => new(0, PhantoonColorFormat.FadeOutCount),
        ("PhantoonColorCatalog", "ResolvePowerOn", "color") => new(0, SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork.PhantoonColorFormat.PoweredPartColourCount),
        ("TourianStatuePalettes", "ResolveEye", "glow") => new(0, TourianStatueColorDefinitions.EyeGlowCount),
        ("TourianStatuePalettes", "ResolveEye", "color") => new(0, TourianStatueColorDefinitions.EyeColorCount),
        ("SporeSpawnColorCatalog", "ResolveSpore" or "ResolveHealth", "color") => new(0, SporeSpawnColorFormat.ColorsPerFrame),
        ("SporeSpawnColorCatalog", "ResolveHealth", "frame") => new(0, SporeSpawnColorDefinitions.HealthFrameCount),
        ("MotherBrainRainbowPalettePresentation", "BeamColor", "index") => new(0,
            MotherBrainRainbowPaletteGeometry.BeamCycleColorCount),
        ("MotherBrainDeathColorCatalog", "BodyColor" or "LegColor", "frame") => new(0, MotherBrainDeathColorFile.BodyFadeFrameCount),
        ("MotherBrainDeathColorCatalog", "BodyColor" or "LegColor", "color") => new(0, MotherBrainDeathColorFormat.BodyColorCount),
        ("MotherBrainDeathColorCatalog", "CorpseColor", "frame") => new(0, MotherBrainDeathColorFile.CorpseFadeFrameCount),
        ("MotherBrainDeathColorCatalog", "CorpseColor", "color") => new(0, MotherBrainDeathColorFormat.CorpseColorCount),
        ("InstalledSamusDeathPalettes", "SuitedColor", "suit") => new(0, InstalledSamusDeathPalettes.SuitCount),
        ("InstalledSamusDeathPalettes", "SuitedColor" or "SuitlessColor", "palette") => new(0, SamusPaletteDefinitions.Death.PaletteCount),
        ("InstalledSamusDeathPalettes", "SuitedColor" or "SuitlessColor", "color") => new(0, InstalledSamusDeathPalettes.ColorCount),
        ("InstalledSamusDeathPalettes", "WhiteoutColor", "index") => new(0, SamusPaletteDefinitions.Death.WhiteoutShadeCount),
        ("InstalledSamusDeathPalettes", "ExplosionPaletteIndex", "frame") => new(0, SamusDeathExplosionTimingDefinitions.RecordCount),
        ("SamusFullBodyCycleColours", "Resolve", "colorIndex") => new(0, SamusFullBodyCycleColorFormat.ColorsPerPalette),
        ("SamusFullBodyCycleColours", "Apply" or "Resolve", "palette.Suit") => new(0, SamusFullBodyCycleColorFormat.SuitCount),
        ("SamusFullBodyCycleColours", "Apply" or "Resolve", "palette.Shade") => new(0, SamusFullBodyCycleColorFormat.ShadesPerSuit),
        ("InstalledSamusSuitColors", "Resolve", "colorIndex") => new(0, SamusSuitColorFormat.ColorsPerSuit),
        ("SamusChargeColours", "ApplyCharge", "suit") => new(0, SamusChargeColorFormat.SuitCount),
        ("SamusChargeColours", "ApplyCharge", "phase") => new(0, SamusChargeColorFormat.PhasesPerSuit),
        ("SamusChargeColours", "ApplyHyper" or "ResolveHyper", "frame") => new(0, SamusChargeColorFormat.HyperFrameCount),
        ("SamusChargeColours", "ResolveHyper", "color") => new(0, SamusChargeColorFormat.ColorsPerPalette),
        ("InstalledSamusHurtColors", "Resolve", "variant") => new(0, Enum.GetValues<SamusHurtColorVariant>().Length),
        ("InstalledSamusHurtColors", "Resolve", "index") => new(0, SamusHurtColorFormat.ColorsPerPalette),
        ("SamusHyperBeamColours", "Apply" or "Resolve", "frame") => new(0, SamusHyperBeamColorFormat.FrameCount),
        ("SamusHyperBeamColours", "Resolve", "color") => new(0, SamusHyperBeamColorFormat.ColorsPerFrame),
        ("InstalledSamusVisorColors", "Resolve", "index") => new(0, SamusVisorColorFormat.ColorCount),
        ("InstalledCrystalFlashColors", "ApplyBody" or "ResolveBody", "frame") => new(0, CrystalFlashColorFormat.BodyFrameCount),
        ("InstalledCrystalFlashColors", "ResolveBody", "color") => new(0, CrystalFlashColorFormat.BodyColorCount),
        ("InstalledCrystalFlashColors", "ApplyBubble" or "ResolveBubble", "frame") => new(0, CrystalFlashColorFormat.BubbleFrameCount),
        ("InstalledCrystalFlashColors", "ResolveBubble", "color") => new(0, CrystalFlashColorFormat.BubbleColorCount),
        ("InstalledPowerBombGlowColors", "Resolve", "sequence") => new(0, Enum.GetValues<PowerBombGlowSequence>().Length),
        ("InstalledPowerBombGlowColors", "Resolve", "index") => new(0, SamusPaletteDefinitions.PowerBomb.ExplosionColorCount),
        ("HyperBeamFxColours", "Apply", "frame") => new(0, SamusColourSets.HyperBeamCycleFrames),
        ("BotwoonColorCatalog", "HealthColor", "band") => new(0, BotwoonHealthBandDefinitions.BandCount),
        ("BotwoonColorCatalog", "HealthColor", "color") => new(0, BotwoonColorFormat.ColorsPerPalette),
        ("BabyMetroidCutsceneColorCatalog", "InitialColor", "color") => new(0, BabyMetroidCutsceneColorFormat.InitialColorCount),
        ("BabyMetroidCutsceneColorCatalog", "FadeColor", "paletteIndex") => new(1, BabyMetroidCutsceneColorDefinitions.FadeFrameCount),
        ("BabyMetroidCutsceneColorCatalog", "FadeColor", "color") => new(0, BabyMetroidCutsceneColorFormat.FadeColorCount),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "frame") => new(0, EnemyAuxiliaryColorDefinitions.All.Max(definition => definition.FrameCount)),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "color") => new(0, EnemyAuxiliaryColorDefinitions.All.Max(definition => definition.ColorCount)),
        ("KraidColorCatalog", "Resolve", "index") => new(0, KraidPalettes.ColorCount(KraidPaletteSource.Health)),
        ("ShitroidColorCatalog", "NormalColor", "frame") => new(0, ShitroidColorDefinitions.NormalFrameCount),
        ("ShitroidColorCatalog", "NormalColor", "color") => new(0, ShitroidColorDefinitions.NormalColorsPerFrame),
        ("ShitroidColorCatalog", "TargetColor", "color") => new(0, ShitroidColorDefinitions.TargetColorCount),
        ("DachoraColorCatalog", "Resolve", "frame") => new(0, DachoraColorDefinitions.AnimatedFrameCount),
        ("DachoraColorCatalog", "Resolve", "color") => new(0, DachoraColorFormat.ColorsPerFrame),
        ("CreditsPresentation", "GetRow", "index") => new(0, CreditsPresentationDefinitions.RowCount),
        ("MapArrowPresentation", "Get", "direction") => new(1, MapArrowDefinitions.Count),
        ("PauseBackdropPresentation", "LoadTo", "area") => new(0, AreaIds.All.Count),
        ("PauseWireframePresentation", "ApplyTo", "kind") => new(0, PauseWireframeDefinitions.Count),
        ("PauseReserveTankPresentation", "Anchor" or "Draw", "index") => new(0, PauseReserveTankDefinitions.AnchorCount),
        ("IntroEyeTilemapPresentation", "FrameTiles", "index") => new(0, IntroEyeTilemapFormat.FrameCount),
        ("CeresDoorVisuals", "AnimationColor", "step") => new(0, CeresDoorVisualDefinitions.AnimationStepCount),
        ("CeresDoorVisuals", "AnimationColor", "color") => new(0, CeresDoorVisualDefinitions.AnimationColorCount),
        ("CeresDoorVisuals", "NormalColor" or "EscapeColor", "color") => new(0, CeresDoorVisualDefinitions.SetupColorCount),
        ("ChargeFlareSprites", "Frame", "selector") => new(0, ChargeFlareSpriteSelection.Selectors.Count),
        ("InstalledGrappleSprites", "Segment", "frame") => new(0, GrappleSpriteDefinitions.SegmentFrameCount),
        ("InstalledSamusBodyArtwork", "GraphicsYOffset", "pose") => new(0, SamusBodySources.PoseCount),
        ("SamusSpritemapSources", "TryGetComposition", "frame.Pose") => new(0, SamusBodySources.PoseCount),
        ("SamusSpritemapSources", "TryGet", "index") => new(0, SamusSpritemapSources.EntryCount),
        ("SamusArmCannonSourceArtwork", "PoseDescriptor", "pose") => new(0, SamusBodySources.PoseCount),
        ("SamusArmCannonSourceArtwork", "ReadDrawingByte", "offset") => new(0, SamusArmCannonArtworkFormat.DrawingDataByteCount),
        ("SamusArmCannonLayout", "PoseDescriptor", "pose") => new(0, SamusBodySources.PoseCount),
        ("SamusArmCannonLayout", "ReadDrawingByte", "offset") => new(0, SamusArmCannonArtworkFormat.DrawingDataByteCount),
        ("SamusArmCannonSourceArtwork", "CoverTileIndex", "direction") => new(0, SamusProjectileDirectionCodec.Count),
        ("SamusArmCannonSourceArtwork", "CoverTileIndex", "frame") => new(0, SamusArmCannonArtworkFormat.FramesPerDirection),
        ("EndingObjectSourceArtwork", "Fragment", "id") => new(0, EndingObjectArtworkFormat.FragmentCount),
        ("GunshipLiftoffArtworkCatalog", "Resolve", "asset") => new((int)InstalledArtworkId.GunshipLiftoffFirstTiles,
            GunshipLiftoffTransferDefinitions.Frames.Length),
        ("AreaMapPresentation" or "IGameplayPresentationContent" or "IAreaMapSource", "Get", "area") => new(0, AreaIds.All.Count),
        _ => null,
    };
}
