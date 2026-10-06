namespace SuperMetroid.ResourceAudit;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["BeamColorWord", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "5069A53D894ABD0A3C888AB8801ED3BB6EEEBBDF33E759FF087936A5779A16C8"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "D82DF9109B8F46520C845F6F51058990DBD365360CF70C07D6A45BAAE8AB2D61"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs", "8FF000D50D77216696DF8B2BB3D75D90BBC90B697FEEEFAAFEAA07325CE6135E"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDrainedPaletteRomData.cs", "FE9739DE495C50D5264953BAF1C871D757E8A147A1FECC9E170407378EFDAAA7"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainFakeDeathPaletteRomData.cs", "C3CC3D0125D864E07ACAFF817517A34519B2B9011261B566BECA227A3EBD736C"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs", "20E95A0BCE5A2E07061C9929E04B040F2486C049CF3529733CA74D7127B9182A")],
            "The sole private-constructor loader requires ten full rainbow rows, eight drain/revival/fake-death rows, normal colors and 38 beam colors. Legacy fake-death rows inherit validated stock. Row selectors are guarded; the aligned terminal beam cursor returns a compiled terminator, not a missing color."),
        new("SuperMetroid.Core.Assets.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs", "D0C1E06270BBB9A84A181EC54A1F6445989557657F958660D7FD7B47512D36F8"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "D82DF9109B8F46520C845F6F51058990DBD365360CF70C07D6A45BAAE8AB2D61"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "5069A53D894ABD0A3C888AB8801ED3BB6EEEBBDF33E759FF087936A5779A16C8"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDeathRomData.cs", "081EFAD19AF9C9D1B9B39DA09362624FB6481417227CA5FDCDC5BD5E9722A96B")],
            "Load compiles all sixteen fourteen-color body/leg rows, eight fifteen-color corpse rows and fourteen exploded-door colors before private construction. Every reviewed resolver bounds-checks the selected complete row/color domain."),
        new("SuperMetroid.Core.Assets.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs", "EE948CC33B289E0DCAB0BD8A905A1AE3189C03180C245C3E9F21DDC90FD2E467"),
             new("csharp/src/SuperMetroid.Core/Game/ChozoAndTubeColorRomData.cs", "6A9A2B10A9A97A66584EC7DD28C562D687E7E1CC45376990A57D2DCB1D4B452B")],
            "Load requires all three fixed 32-color images before private construction. These Apply operations transfer only the corresponding loaded array; raw color resolvers are not covered by this rule."),
        new("SuperMetroid.Core.Assets.GameplayBasePaletteCatalog", "gameplay-base-v1-complete-cgram-image",
            ["LoadInitial", "LoadCommonSprites", "LoadEnemyProjectileSprites"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs", "32E6AD653BB4FFDB35EEF152F313DAD1C37D53C7F49A399BD0BAB9A2B2120697"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesCgram.cs", "68601F80352793660D9795194E8EB56D9B9906E775F0E1597682F1BC3D4A5973"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesPpuLayout.cs", "A880E1FC838925490AEEFBC62E19495CA9311EF9D329C39BB234EC78109C2E4E")],
            "Load compiles the full 256-color initial image and sixteen common sprite colors before private construction. Projectile colors are the installed initial-image slice 208..223. Destination arguments select CGRAM placement, never another resource identity."),
        new("SuperMetroid.Core.Assets.SamusDeathPaletteArtworkCatalog", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusDeathPaletteArtworkCatalog.cs", "680EFE2E59171A969FC2B9960B4BED8137C36298AD4170806AAB135908E24750"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "4CBE656DC00C6CF33257B12449042328AB3C97DA6DD1901816F82B138BBC7C23"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs", "CADFA0A1E5CC12FCF1EC6868D0EF7FEEAFF1DC1816046CF0C90F7FCACBA65325"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "452B184D6FC9F55826EBDF8AB90EDA286A900B50B4CA0633C79D065B063E6F9C")],
            "The public constructor validates all three ten-row suited families, ten suitless rows, 22 whiteout shades and nine explosion palette indices. Suited and suitless fades calculate RGB5 eighth steps with independently copied inputs and channel overrides; uniform flash/final rows share color inputs and explosion selectors skip the flash-only row with explicit edited overrides. Suitless middle tint inks calculate shared endpoint color balance from supplied blue intensities, preserving whole edited colors when RGB would overflow. Suitless warm middle inks calculate downward-rounded RGB thirds from their endpoints. Suitless gray inks calculate upward-rounded fifth steps toward black; neutral base/final roots store one intensity with independent channel overrides. Whiteout interpolates two neutral segments from copied endpoint intensities and keeps independent edited channels. Resolvers preserve complete valid domains and bounds rejection; this rule does not prove callers keep dynamic indices in range."),
    ];
}
