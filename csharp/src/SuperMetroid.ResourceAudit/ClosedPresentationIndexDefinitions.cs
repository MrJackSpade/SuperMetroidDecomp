using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.ResourceAudit;

internal readonly record struct ProviderIndexDomain(int First, int Count, int Stride = 1)
{
    internal bool Contains(int value) => value >= First && (value - First) % Stride == 0 && (value - First) / Stride < Count;
    internal string Description => $"{First}..{First + (Count - 1) * Stride}, stride {Stride}";
}

/// <summary>Finite index domains from the source-reviewed loader/selector contracts.</summary>
internal static class ClosedPresentationIndexDefinitions
{
    internal static ProviderIndexDomain? Get(string type, string method, string parameter) => (type, method, parameter) switch
    {
        ("GameplayHudPresentation", "TryApplyIcon", "itemIndex") => new(0, 5),
        ("GameplayHudPresentation", "ApplyAmmo", "itemIndex") => new(0, 3),
        ("GameplayHudPresentation", "MinimapCellIndex", "outputX") => new(0, 5),
        ("GameplayHudPresentation", "MinimapCellIndex", "outputY") => new(0, 3),
        ("FileSelectPresentation", "WriteDigit", "digit") => new(0, 10),
        ("FileSelectPresentation", "Slot" or "WriteSlotLetter" or "DrawHelmet", "slot") => new(0, 3),
        ("FileSelectPresentation", "DrawCursor", "frame") => new(0, 4),
        ("FileSelectPresentation", "DrawHelmet", "frame") => new(0, 8),
        ("FileSelectPresentation", "CursorPosition", "selected") => new(0, 6),
        ("MotherBrainRoomColorPresentation", "ApplyRecoveryLights", "frame") => new(0, MotherBrainRoomColorRomData.RecoveryLightsFrames),
        ("MotherBrainRoomColorPresentation", "ApplyFlash", "timedEntryPointer") => new(MotherBrainRoomPaletteProgramDefinitions.FlashStart,
            MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount, MotherBrainRoomColorRomData.TimedEntryByteCount),
        ("GameOptionsPresentation", "ApplyControllerLabel", "action" or "button") => new(0, 7),
        ("GameOptionsPresentation", "DrawCursor", "frame") => new(0, 4),
        ("GameOverPresentation", "DrawBaby", "frame") => new(0, 3),
        ("GameOverPresentation", "DrawCursor", "frame") => new(0, 4),
        ("GameOverPresentation", "ApplyBabyPalette", "palette") => new(0, 4),
        ("PauseReserveUiPresentation", "ApplyDigit", "position") => new(0, PauseReserveUiDefinitions.SupplyDigitPlaces),
        ("PauseReserveUiPresentation", "ApplyDigit", "value") => new(0, PauseReserveUiDefinitions.DigitCount),
        ("BeamPaletteCatalog", "LoadTo", "selection") => new(0, BeamTileAtlasDefinitions.SelectionCount),
        ("CeresRidleyColorCatalog", "ApplyEyeFade", "row") => new(0, CeresRidleyPaletteRomData.EyeFadeRowCount),
        ("CeresRidleyColorCatalog", "ApplyBodyFade", "row") => new(0, CeresRidleyPaletteRomData.BodyFadeRowCount),
        ("CeresRidleyColorCatalog", "ApplyHealth", "row") => new(0, CeresRidleyPaletteRomData.HealthRowCount),
        ("CeresRidleyColorCatalog", "ApplyAlarm", "row") => new(0, CeresRidleyPaletteRomData.AlarmRowCount),
        ("CeresRidleyColorCatalog", "ApplyBaby", "row") => new(0, CeresRidleyPaletteRomData.BabyRowCount),
        ("DraygonColorCatalog", "ApplyHealthBand", "tableByteIndex") => new(0, DraygonColorRomData.HealthBandCount, sizeof(ushort)),
        ("DraygonColorCatalog", "ApplyHurt", "healthTableByteIndex") => new(0, DraygonColorRomData.HealthBandCount, sizeof(ushort)),
        ("PhantoonColorCatalog", "ResolveHealth", "band") => new(0, PhantoonColorRomData.HealthBandCount),
        ("PhantoonColorCatalog", "ResolveHealth", "color") => new(0, PhantoonColorRomData.HealthBandColorCount),
        ("PhantoonColorCatalog", "ResolveFadeOut", "color") => new(0, PhantoonColorRomData.FadeOutCount),
        ("PhantoonColorCatalog", "ResolvePowerOn", "color") => new(0, PhantoonColorRomData.PowerOnCount),
        ("TourianStatueColorCatalog", "ApplyEye", "doubledBossParameter") => new(0, TourianStatuePaletteRomData.EyeRowCount, sizeof(ushort)),
        ("SporeSpawnColorCatalog", "ResolveSpore" or "ResolveHealth" or "ResolveDeath", "color") => new(0, SporeSpawnColorRomData.ColorsPerFrame),
        ("SporeSpawnColorCatalog", "ResolveHealth", "frame") => new(0, SporeSpawnColorRomData.HealthFrameCount),
        ("SporeSpawnColorCatalog", "ResolveDeath", "layer") => new(0, 3),
        // The union is eight rows; the selected non-sprite layer still enforces
        // its narrower seven-row domain inside the reviewed provider.
        ("SporeSpawnColorCatalog", "ResolveDeath", "frame") => new(0, SporeSpawnColorRomData.DeathSpriteFrameCount),
        ("MotherBrainRainbowPalettePresentation", "BeamColorWord", "byteCursor") => new(0,
            MotherBrainRainbowPaletteFormat.BeamCycleColorCount + 1, MotherBrainBeamRomData.ColorStride),
        ("MotherBrainRainbowPalettePresentation", "ApplyRainbow", "frame") => new(0, MotherBrainRainbowPaletteFormat.RainbowFrameCount),
        ("MotherBrainRainbowPalettePresentation", "ApplyToGrey" or "ApplyFromGrey" or "ApplyFakeDeathFromGrey", "frame") => new(0, MotherBrainRainbowPaletteFormat.GreyFrameCount),
        ("MotherBrainRainbowPalettePresentation", "ApplyFakeDeathToGrey", "frame") => new(0, MotherBrainFakeDeathPaletteRomData.FrameCount),
        ("MotherBrainDeathColorCatalog", "BodyColor" or "LegColor", "frame") => new(0, MotherBrainDeathRomData.BodyFadeFrameCount),
        ("MotherBrainDeathColorCatalog", "BodyColor" or "LegColor" or "ExplodedDoorColor", "color") => new(0, MotherBrainDeathRomData.BodyColorCount),
        ("MotherBrainDeathColorCatalog", "CorpseColor", "frame") => new(0, MotherBrainDeathRomData.CorpseFadeFrameCount),
        ("MotherBrainDeathColorCatalog", "CorpseColor", "color") => new(0, MotherBrainDeathRomData.CorpseColorCount),
        ("SamusDeathPaletteArtworkCatalog", "SuitedColor", "suit") => new(0, SamusDeathPaletteArtworkCatalog.SuitCount),
        ("SamusDeathPaletteArtworkCatalog", "SuitedColor" or "SuitlessColor", "palette") => new(0, SamusPaletteRomData.Death.PaletteCount),
        ("SamusDeathPaletteArtworkCatalog", "SuitedColor" or "SuitlessColor", "color") => new(0, SamusDeathPaletteArtworkCatalog.ColorCount),
        ("SamusDeathPaletteArtworkCatalog", "WhiteoutColor", "index") => new(0, SamusPaletteRomData.Death.WhiteoutShadeCount),
        ("SamusDeathPaletteArtworkCatalog", "ExplosionPaletteIndex", "frame") => new(0, SamusDeathExplosionTimingDefinitions.RecordCount),
        ("SamusFullBodyCycleColorCatalog", "Resolve", "colorIndex") => new(0, SamusFullBodyCycleColorFormat.ColorsPerPalette),
        ("SamusSuitColorCatalog", "Resolve", "colorIndex") => new(0, SamusSuitColorFormat.ColorsPerSuit),
        ("SamusChargeColorCatalog", "ApplyCharge" or "ResolveCharge", "suit") => new(0, SamusChargeColorFormat.SuitCount),
        ("SamusChargeColorCatalog", "ApplyCharge" or "ResolveCharge", "phase") => new(0, SamusChargeColorFormat.PhasesPerSuit),
        ("SamusChargeColorCatalog", "ApplyHyper" or "ResolveHyper", "frame") => new(0, SamusChargeColorFormat.HyperFrameCount),
        ("SamusChargeColorCatalog", "ResolveCharge" or "ResolveHyper", "color") => new(0, SamusChargeColorFormat.ColorsPerPalette),
        ("SamusHurtColorCatalog", "Resolve", "variant") => new(0, Enum.GetValues<SamusHurtColorVariant>().Length),
        ("SamusHurtColorCatalog", "Resolve", "index") => new(0, SamusHurtColorFormat.ColorsPerPalette),
        ("SamusHyperBeamColorCatalog", "Resolve", "frame") => new(0, SamusHyperBeamColorFormat.FrameCount),
        ("SamusHyperBeamColorCatalog", "Resolve", "color") => new(0, SamusHyperBeamColorFormat.ColorsPerFrame),
        ("SamusVisorColorCatalog", "Resolve", "index") => new(0, SamusVisorColorFormat.ColorCount),
        // Unsupported TryResolveByteOffset values validly return false; this
        // membership query does not demand a seventh or unaligned color.
        ("CrystalFlashColorCatalog", "ApplyBody" or "ResolveBody", "frame") => new(0, CrystalFlashColorFormat.BodyFrameCount),
        ("CrystalFlashColorCatalog", "ResolveBody", "color") => new(0, CrystalFlashColorFormat.BodyColorCount),
        ("CrystalFlashColorCatalog", "ApplyBubble" or "ResolveBubble", "frame") => new(0, CrystalFlashColorFormat.BubbleFrameCount),
        ("CrystalFlashColorCatalog", "ResolveBubble", "color") => new(0, CrystalFlashColorFormat.BubbleColorCount),
        ("PowerBombFixedColorCatalog", "Resolve", "sequence") => new(0, Enum.GetValues<PowerBombFixedColorSequence>().Length),
        ("PowerBombFixedColorCatalog", "Resolve", "index") => new(0, SamusPaletteRomData.PowerBomb.ExplosionColorCount),
        ("HyperBeamFxColorCatalog", "Apply", "frame") => new(0, HyperBeamFxColorFormat.FrameCount),
        ("BotwoonColorCatalog", "HealthColor", "band") => new(0, BotwoonHealthPaletteDefinitions.PaletteCount),
        ("BotwoonColorCatalog", "HealthColor", "color") => new(0, BotwoonHealthPaletteDefinitions.ColorsPerPalette),
        ("BabyMetroidCutsceneColorCatalog", "InitialColor", "color") => new(0, BabyMetroidCutsceneColorRomData.InitialColorCount),
        ("BabyMetroidCutsceneColorCatalog", "FadeColor", "paletteIndex") => new(1, BabyMetroidCutsceneColorRomData.FadeFrameCount),
        ("BabyMetroidCutsceneColorCatalog", "FadeColor", "color") => new(0, BabyMetroidCutsceneColorRomData.FadeColorCount),
        ("NorfairRidleyColorCatalog", "ApplyReveal" or "ResolveReveal", "row") => new(0, NorfairRidleyPaletteRomData.RevealRowCount),
        ("NorfairRidleyColorCatalog", "ResolveInitial", "color") => new(0, NorfairRidleyPaletteRomData.InitialColorCount),
        ("NorfairRidleyColorCatalog", "ResolveReveal", "color") => new(0, NorfairRidleyPaletteRomData.RevealColorCount),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "frame") => new(0, EnemyAuxiliaryColorDefinitions.All.ToArray().Max(definition => definition.FrameCount)),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "color") => new(0, EnemyAuxiliaryColorDefinitions.All.ToArray().Max(definition => definition.ColorCount)),
        ("KraidColorCatalog", "Resolve", "index") => new(0, KraidPaletteRomData.ColorCount(KraidPaletteSource.Health)),
        ("ZebetiteColorCatalog", "Apply", "frame") => new(0, ZebetiteColorFormat.FrameCount),
        ("ShitroidColorCatalog", "NormalColor", "frame") => new(0, ShitroidColorRomData.NormalFrameCount),
        ("ShitroidColorCatalog", "NormalColor", "color") => new(0, ShitroidColorRomData.NormalColorsPerFrame),
        ("ShitroidColorCatalog", "TargetColor", "color") => new(0, ShitroidColorRomData.TargetColorCount),
        ("CeresRidleyMode7ColorCatalog", "Apply" or "Resolve", "zoomHighByte") => new(0, CeresRidleyPaletteRomData.Mode7ZoomRowCount),
        ("CeresRidleyMode7ColorCatalog", "Resolve", "color") => new(0, CeresRidleyPaletteRomData.Mode7ZoomColorCount),
        ("DachoraColorCatalog", "Resolve", "frame") => new(0, DachoraColorRomData.AnimatedFrameCount),
        ("DachoraColorCatalog", "Resolve", "color") => new(0, DachoraColorRomData.ColorsPerFrame),
        ("MotherBrainHealthPalettePresentation", "Apply", "damageState") => new(0, MotherBrainHealthPaletteFormat.StateCount),
        ("CreditsPresentation", "GetRow", "index") => new(0, CreditsPresentationDefinitions.ExpectedCompiledRows),
        ("MapArrowPresentation", "Get", "direction") => new(1, MapArrowDefinitions.Count),
        ("PauseBackdropPresentation", "LoadTo", "area") => new(0, AreaIds.RetailCount),
        ("PauseWireframePresentation", "ApplyTo", "kind") => new(0, PauseWireframeDefinitions.Count),
        ("PauseReserveTankPresentation", "Anchor" or "Draw", "index") => new(0, PauseReserveTankDefinitions.AnchorCount),
        _ => null,
    };
}
