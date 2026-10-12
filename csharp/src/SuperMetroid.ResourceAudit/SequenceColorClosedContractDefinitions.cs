namespace SuperMetroid.ResourceAudit;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    /// <summary>Imported palette sequences and fixed color sets reviewed as
    /// complete presentation domains, with their source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["TryReadBeamColor", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDrainedPaletteRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainFakeDeathPaletteRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs")]),
        new("SuperMetroid.Core.Assets.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDeathRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainExplodedDoorPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/ChozoAndTubeColorRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStatuePaintDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.GameplayBasePaletteCatalog", "gameplay-base-v1-complete-cgram-image",
            ["LoadInitial", "LoadCommonSprites", "LoadEnemyProjectileSprites"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesCgram.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesPpuLayout.cs")]),
        new("SuperMetroid.Core.Assets.SamusDeathPaletteArtworkCatalog", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusDeathPaletteArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs")]),
    ];
}
