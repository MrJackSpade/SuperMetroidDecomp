using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Finite index domains from the source-reviewed loader/selector contracts.</summary>
internal static class ClosedPresentationIndexDefinitions
{
    /// <summary>Returns a source-reviewed finite domain for a provider parameter, or null when no closed numeric proof exists.</summary>
    internal static ProviderIndexDomain? Get(string type, string method, string parameter) => new ClosedPresentationIndexGetCase(type, method, parameter) switch
    {
        ("GameplayHudPresentation", "TryApplyIcon", "itemIndex") => new(0, 5),
        ("GameplayHudPresentation", "ApplyAmmo", "itemIndex") => new(0, 3),
        ("GameplayHudPresentation", "MinimapCellIndex", "output.X") => new(0, GameplayHudPresentation.MinimapSize.X),
        ("GameplayHudPresentation", "MinimapCellIndex", "output.Y") => new(0, GameplayHudPresentation.MinimapSize.Y),
        ("FileSelectPresentation", "WriteDigit", "digit") => new(0, 10),
        ("FileSelectPresentation", "Slot" or "WriteSlotLetter" or "DrawHelmet", "slot") => new(0, 3),
        ("FileSelectPresentation", "DrawCursor", "frame") => new(0, 4),
        ("FileSelectPresentation", "DrawHelmet", "frame") => new(0, 8),
        ("FileSelectPresentation", "CursorPosition", "selected") => new(0, 6),
        ("MotherBrainRoomColorPresentation", "ApplyRecoveryLights", "frame") => new(0, MotherBrainRoomColorDefinitions.RecoveryLightsFrames),
        ("MotherBrainRoomColorPresentation", "ApplyFlash", "frame") => new(0, MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount),
        ("GameOptionsPresentation", "ApplyControllerLabel", "action" or "button") => new(0, 7),
        ("GameOptionsPresentation", "DrawCursor", "frame") => new(0, 4),
        ("GameOverPresentation", "DrawBaby", "frame") => new(0, 3),
        ("GameOverPresentation", "DrawCursor", "frame") => new(0, 4),
        ("GameOverPresentation", "ApplyBabyPalette", "palette") => new(0, 4),
        ("PauseReserveUiPresentation", "ApplyDigit", "position") => new(0, PauseReserveUiDefinitions.SupplyDigitPlaces),
        ("PauseReserveUiPresentation", "ApplyDigit", "value") => new(0, PauseReserveUiDefinitions.DigitCount),
        ("InstalledBeamPalettes", "LoadTo", "selection") => new(0, BeamPaletteDefinitions.SelectionCount),
        ("CeresRidleyColorCatalog", "ApplyEyeFade", "row") => new(0, CeresRidleyPaletteDefinitions.EyeFadeRowCount),
        ("CeresRidleyColorCatalog", "ApplyBodyFade", "row") => new(0, CeresRidleyPaletteDefinitions.BodyFadeRowCount),
        ("CeresRidleyColorCatalog", "ApplyHealth", "row") => new(0, CeresRidleyPaletteDefinitions.HealthRowCount),
        ("CeresRidleyColorCatalog", "ApplyAlarm", "row") => new(0, CeresRidleyPaletteDefinitions.AlarmRowCount),
        ("CeresRidleyColorCatalog", "ApplyBaby", "row") => new(0, CeresRidleyPaletteDefinitions.BabyRowCount),
        ("DraygonColorCatalog", "ApplyHealthBand", "band") => new(0, DraygonColorDefinitions.HealthBandCount),
        ("DraygonColorCatalog", "ApplyHurt", "healthBand") => new(0, DraygonColorDefinitions.HealthBandCount),
        ("PhantoonColorCatalog", "ResolveHealth", "band") => new(0, PhantoonColorDefinitions.HealthBandCount),
        ("PhantoonColorCatalog", "ResolveHealth", "color") => new(0, PhantoonColorDefinitions.HealthBandColorCount),
        ("PhantoonColorCatalog", "ResolveFadeOut", "color") => new(0, PhantoonColorDefinitions.FadeOutCount),
        ("PhantoonColorCatalog", "ResolvePowerOn", "color") => new(0, PhantoonColorDefinitions.PowerOnCount),
        ("TourianStatuePalettes", "ApplyEye", "doubledBossParameter") => new(0, TourianStatueColorDefinitions.EyeRowCount, sizeof(ushort)),
        ("SporeSpawnColorCatalog", "ResolveSpore" or "ResolveHealth" or "ResolveDeath", "color") => new(0, SporeSpawnColorDefinitions.ColorsPerFrame),
        ("SporeSpawnColorCatalog", "ResolveHealth", "frame") => new(0, SporeSpawnColorDefinitions.HealthFrameCount),
        ("SporeSpawnColorCatalog", "ResolveDeath", "layer") => new(0, 3),
        // The union is eight rows; the selected non-sprite layer still enforces
        // its narrower seven-row domain inside the reviewed provider.
        ("SporeSpawnColorCatalog", "ResolveDeath", "frame") => new(0, SporeSpawnColorDefinitions.DeathSpriteFrameCount),
        ("MotherBrainRainbowPalettePresentation", "TryReadBeamColor", "index") => new(0,
            MotherBrainRainbowPaletteFormat.BeamCycleColorCount + 1),
        ("MotherBrainRainbowPalettePresentation", "ApplyRainbow", "frame") => new(0, MotherBrainRainbowPaletteFormat.RainbowFrameCount),
        ("MotherBrainRainbowPalettePresentation", "ApplyToGrey" or "ApplyFromGrey" or "ApplyFakeDeathFromGrey", "frame") => new(0, MotherBrainRainbowPaletteFormat.GreyFrameCount),
        ("MotherBrainRainbowPalettePresentation", "ApplyFakeDeathToGrey", "frame") => new(0, MotherBrainFakeDeathPaletteDefinitions.FrameCount),
        ("MotherBrainDeathColorCatalog", "BodyColor" or "LegColor", "frame") => new(0, MotherBrainDeathDefinitions.BodyFadeFrameCount),
        ("MotherBrainDeathColorCatalog", "BodyColor" or "LegColor" or "ExplodedDoorColor", "color") => new(0, MotherBrainDeathDefinitions.BodyColorCount),
        ("MotherBrainDeathColorCatalog", "CorpseColor", "frame") => new(0, MotherBrainDeathDefinitions.CorpseFadeFrameCount),
        ("MotherBrainDeathColorCatalog", "CorpseColor", "color") => new(0, MotherBrainDeathDefinitions.CorpseColorCount),
        ("InstalledSamusDeathPalettes", "SuitedColor", "suit") => new(0, InstalledSamusDeathPalettes.SuitCount),
        ("InstalledSamusDeathPalettes", "SuitedColor" or "SuitlessColor", "palette") => new(0, SamusPaletteDefinitions.Death.PaletteCount),
        ("InstalledSamusDeathPalettes", "SuitedColor" or "SuitlessColor", "color") => new(0, InstalledSamusDeathPalettes.ColorCount),
        ("InstalledSamusDeathPalettes", "WhiteoutColor", "index") => new(0, SamusPaletteDefinitions.Death.WhiteoutShadeCount),
        ("InstalledSamusDeathPalettes", "ExplosionPaletteIndex", "frame") => new(0, SamusDeathExplosionTimingDefinitions.RecordCount),
        ("InstalledSamusFullBodyCycleColors", "Resolve", "colorIndex") => new(0, SamusFullBodyCycleColorFormat.ColorsPerPalette),
        ("InstalledSamusFullBodyCycleColors", "Apply" or "Resolve", "palette.Suit") => new(0, SamusFullBodyCycleColorFormat.SuitCount),
        ("InstalledSamusFullBodyCycleColors", "Apply" or "Resolve", "palette.Shade") => new(0, SamusFullBodyCycleColorFormat.ShadesPerSuit),
        ("InstalledSamusSuitColors", "Resolve", "colorIndex") => new(0, SamusSuitColorFormat.ColorsPerSuit),
        ("InstalledSamusChargeColors", "ApplyCharge" or "ResolveCharge", "suit") => new(0, SamusChargeColorFormat.SuitCount),
        ("InstalledSamusChargeColors", "ApplyCharge" or "ResolveCharge", "phase") => new(0, SamusChargeColorFormat.PhasesPerSuit),
        ("InstalledSamusChargeColors", "ApplyHyper" or "ResolveHyper", "frame") => new(0, SamusChargeColorFormat.HyperFrameCount),
        ("InstalledSamusChargeColors", "ResolveCharge" or "ResolveHyper", "color") => new(0, SamusChargeColorFormat.ColorsPerPalette),
        ("InstalledSamusHurtColors", "Resolve", "variant") => new(0, Enum.GetValues<SamusHurtColorVariant>().Length),
        ("InstalledSamusHurtColors", "Resolve", "index") => new(0, SamusHurtColorFormat.ColorsPerPalette),
        ("InstalledSamusHyperBeamColors", "Resolve", "frame") => new(0, SamusHyperBeamColorFormat.FrameCount),
        ("InstalledSamusHyperBeamColors", "Resolve", "color") => new(0, SamusHyperBeamColorFormat.ColorsPerFrame),
        ("InstalledSamusVisorColors", "Resolve", "index") => new(0, SamusVisorColorFormat.ColorCount),
        // Unsupported TryResolveByteOffset values validly return false; this
        // membership query does not demand a seventh or unaligned color.
        ("InstalledCrystalFlashColors", "ApplyBody" or "ResolveBody", "frame") => new(0, CrystalFlashColorFormat.BodyFrameCount),
        ("InstalledCrystalFlashColors", "ResolveBody", "color") => new(0, CrystalFlashColorFormat.BodyColorCount),
        ("InstalledCrystalFlashColors", "ApplyBubble" or "ResolveBubble", "frame") => new(0, CrystalFlashColorFormat.BubbleFrameCount),
        ("InstalledCrystalFlashColors", "ResolveBubble", "color") => new(0, CrystalFlashColorFormat.BubbleColorCount),
        ("InstalledPowerBombFixedColors", "Resolve", "sequence") => new(0, Enum.GetValues<PowerBombFixedColorSequence>().Length),
        ("InstalledPowerBombFixedColors", "Resolve", "index") => new(0, SamusPaletteDefinitions.PowerBomb.ExplosionColorCount),
        ("InstalledHyperBeamFxColors", "Apply", "frame") => new(0, HyperBeamFxColorFormat.FrameCount),
        ("BotwoonColorCatalog", "HealthColor", "band") => new(0, BotwoonHealthPaletteDefinitions.PaletteCount),
        ("BotwoonColorCatalog", "HealthColor", "color") => new(0, BotwoonHealthPaletteDefinitions.ColorsPerPalette),
        ("BabyMetroidCutsceneColorCatalog", "InitialColor", "color") => new(0, BabyMetroidCutsceneColorDefinitions.InitialColorCount),
        ("BabyMetroidCutsceneColorCatalog", "FadeColor", "paletteIndex") => new(1, BabyMetroidCutsceneColorDefinitions.FadeFrameCount),
        ("BabyMetroidCutsceneColorCatalog", "FadeColor", "color") => new(0, BabyMetroidCutsceneColorDefinitions.FadeColorCount),
        ("NorfairRidleyColorCatalog", "ApplyReveal" or "ResolveReveal", "row") => new(0, NorfairRidleyPaletteDefinitions.RevealRowCount),
        ("NorfairRidleyColorCatalog", "ResolveInitial", "color") => new(0, NorfairRidleyPaletteDefinitions.InitialColorCount),
        ("NorfairRidleyColorCatalog", "ResolveReveal", "color") => new(0, NorfairRidleyPaletteDefinitions.RevealColorCount),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "frame") => new(0, EnemyAuxiliaryColorDefinitions.All.Max(definition => definition.FrameCount)),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "color") => new(0, EnemyAuxiliaryColorDefinitions.All.Max(definition => definition.ColorCount)),
        ("KraidColorCatalog", "Resolve", "index") => new(0, KraidPalettes.ColorCount(KraidPaletteSource.Health)),
        ("ZebetiteColorCatalog", "Apply", "frame") => new(0, ZebetiteColorFormat.FrameCount),
        ("ShitroidColorCatalog", "NormalColor", "frame") => new(0, ShitroidColorDefinitions.NormalFrameCount),
        ("ShitroidColorCatalog", "NormalColor", "color") => new(0, ShitroidColorDefinitions.NormalColorsPerFrame),
        ("ShitroidColorCatalog", "TargetColor", "color") => new(0, ShitroidColorDefinitions.TargetColorCount),
        ("CeresRidleyFloorColorCatalog", "Apply" or "Resolve", "zoomHighByte") => new(0, CeresRidleyPaletteDefinitions.GetawayZoomRowCount),
        ("CeresRidleyFloorColorCatalog", "Resolve", "color") => new(0, CeresRidleyPaletteDefinitions.GetawayZoomColorCount),
        ("DachoraColorCatalog", "Resolve", "frame") => new(0, DachoraColorDefinitions.AnimatedFrameCount),
        ("DachoraColorCatalog", "Resolve", "color") => new(0, DachoraColorDefinitions.ColorsPerFrame),
        ("MotherBrainHealthPalettePresentation", "Apply", "damageState") => new(0, MotherBrainHealthPaletteFormat.StateCount),
        ("CreditsPresentation", "GetRow", "index") => new(0, CreditsPresentationDefinitions.ExpectedCompiledRows),
        ("MapArrowPresentation", "Get", "direction") => new(1, MapArrowDefinitions.Count),
        ("PauseBackdropPresentation", "LoadTo", "area") => new(0, AreaIds.RetailCount),
        ("PauseWireframePresentation", "ApplyTo", "kind") => new(0, PauseWireframeDefinitions.Count),
        ("PauseReserveTankPresentation", "Anchor" or "Draw", "index") => new(0, PauseReserveTankDefinitions.AnchorCount),
        ("IntroEyeTilemapPresentation", "FrameTiles", "index") => new(0, IntroEyeTilemapFormat.FrameCount),
        ("CeresDoorVisuals", "LoadAnimationColors", "row") => new(0, CeresDoorVisualDefinitions.AnimationRowCount),
        ("CeresDoorVisuals", "PlatformFrame", "frame") => new(0, CeresDoorVisualDefinitions.PlatformFrameCount),
        ("RoomSkyTilemaps", "Row" or "Page", "page") => new(0, RoomSkyTilemapFormat.PageCount),
        ("RoomSkyTilemaps", "Row", "row") => new(0, RoomSkyTilemapFormat.RowsPerPage),
        ("ChargeFlarePlacements", "Resolve", "direction") => new(0, ChargeFlarePlacementDefinitions.DirectionCount),
        ("ChargeFlareSprites", "Frame", "selector") => new(0, ChargeFlareSpriteSelection.Selectors.Count),
        ("InstalledGrappleSprites", "Segment", "frame") => new(0, GrappleSpriteDefinitions.SegmentFrameCount),
        ("InstalledSamusBodyArtwork", "GraphicsYOffset", "pose") => new(0, InstalledSamusBodyArtwork.PoseCount),
        ("InstalledSamusSpritemaps", "TopBase" or "BottomBase", "pose") => new(0, InstalledSamusBodyArtwork.PoseCount),
        ("InstalledSamusSpritemaps", "TryGet" or "TryGetComposition", "index") => new(0, InstalledSamusSpritemaps.EntryCount),
        ("InstalledSamusArmCannonArtwork", "PoseDescriptor", "pose") => new(0, InstalledSamusBodyArtwork.PoseCount),
        ("InstalledSamusArmCannonArtwork", "ReadDrawingByte", "offset") => new(0, SamusArmCannonArtworkFormat.DrawingDataByteCount),
        ("InstalledSamusArmCannonArtwork", "CoverTileIndex", "direction") => new(0, SamusDrawingDefinitions.ArmCannon.DirectionCount),
        ("InstalledSamusArmCannonArtwork", "CoverTileIndex", "frame") => new(0, SamusArmCannonArtworkFormat.FramesPerDirection),
        ("EndingObjectArtwork", "Fragment", "id") => new(0, EndingObjectArtworkFormat.FragmentCount),
        ("GunshipLiftoffArtworkCatalog", "Resolve", "asset") => new((int)InstalledArtworkId.GunshipLiftoffFirstTiles,
            GunshipLiftoffTransferDefinitions.Frames.Length),
        ("AreaMapPresentation" or "IGameplayPresentationContent" or "IAreaMapSource", "Get", "area") => new(0, AreaIds.RetailCount),
        ("InstalledXrayOverlays", "ItemMetatile", "graphicsSlot") => new(0, SuperMetroid.Core.Samus.Abilities.Artwork.InstalledXrayOverlays.DynamicGraphicsSlots * 2),
        ("InstalledDynamicCollectibleArt", "Resolve", "kind") => new((int)InWorldCollectibleKind.Bombs,
            (int)InWorldCollectibleKind.ReserveTank - (int)InWorldCollectibleKind.Bombs + 1),
        _ => null,
    };
}
