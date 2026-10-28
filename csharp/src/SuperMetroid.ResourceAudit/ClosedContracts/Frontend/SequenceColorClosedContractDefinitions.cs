namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    /// <summary>Imported palette sequences and fixed color sets reviewed as
    /// complete presentation domains, with their source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["TryReadBeamColor", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            [new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRainbowPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRainbowShadeChannel.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainHealthPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainDrainedPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainRainbowPaletteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainDrainedPaletteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathPaletteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainBeamDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainDeathColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainHealthPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainDrainedPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRainbowPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRainbowShadeChannel.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainDeathDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainExplodedDoorPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainFinalRoomPaintDefinitions.cs")]),
        new("SuperMetroid.Core.Enemies.Artwork.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/ChozoAndTubeColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoAndTubeColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/ChozoStatuePaintDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.GameplayBasePalette", "gameplay-base-v1-complete-cgram-image",
            ["LoadInitial", "LoadCommonSprites", "LoadEnemyProjectileSprites"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayBasePalette.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.SamusDeathPaletteArtworkCatalog", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusDeathPaletteArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusPaletteFade.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Palettes/SamusPaletteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs")]),
    ];
}
