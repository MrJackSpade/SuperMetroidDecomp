namespace SuperMetroid.ResourceAudit;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["BeamColorWord", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "3FCF60FDBF1D65214281DBE1CD890280D02054E46785A843AF8BDDD79780C0A0"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs", "8FF000D50D77216696DF8B2BB3D75D90BBC90B697FEEEFAAFEAA07325CE6135E"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDrainedPaletteRomData.cs", "FE9739DE495C50D5264953BAF1C871D757E8A147A1FECC9E170407378EFDAAA7"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainFakeDeathPaletteRomData.cs", "C3CC3D0125D864E07ACAFF817517A34519B2B9011261B566BECA227A3EBD736C"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs", "DA646818AD3F58900A9209F33B59952D4239B56B8EB7DAA24A19A2442E2FD44F")],
            "The sole private-constructor loader requires ten full rainbow rows, eight drain/revival/fake-death rows, normal colors and 38 beam colors. Legacy fake-death rows inherit validated stock. Row selectors are guarded; the aligned terminal beam cursor returns a compiled terminator, not a missing color."),
        new("SuperMetroid.Core.Assets.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs", "ECF27902773206B741220DB9BD0B94F7E6CB8F69AF591DFF606370EE0E76AC99"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDeathRomData.cs", "081EFAD19AF9C9D1B9B39DA09362624FB6481417227CA5FDCDC5BD5E9722A96B")],
            "Load compiles all sixteen fourteen-color body/leg rows, eight fifteen-color corpse rows and fourteen exploded-door colors before private construction. Every reviewed resolver bounds-checks the selected complete row/color domain."),
        new("SuperMetroid.Core.Assets.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs", "7DF5D85B8486C8D5B04947D5B46FE68786B6EEA8E813EF304785B6C7C9EE2495"),
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
            [new("csharp/src/SuperMetroid.Core/Assets/SamusDeathPaletteArtworkCatalog.cs", "E15DF5E6F33B69ED8128413D979D3CC3FAD1D3EF7DE6A6F8D54AC6A9DAB62B70"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "4CBE656DC00C6CF33257B12449042328AB3C97DA6DD1901816F82B138BBC7C23"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs", "8CDA69E0630B7D021334750B8735DD18EE309008DD0982365D5A924F0E226EDC"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "23A3E27001C5B6A284DD0B4CD1152C0329634713075698820367010D64FC99C8")],
            "The public constructor validates all three ten-row suited families, ten suitless rows, 22 whiteout shades and nine explosion palette indices. Suited and suitless fades calculate RGB5 eighth steps with independently copied inputs and channel overrides; uniform flash/final rows share color inputs and explosion selectors skip the flash-only row with explicit edited overrides. Suitless middle tint inks calculate shared endpoint color balance from supplied blue intensities, preserving whole edited colors when RGB would overflow. Suitless warm middle inks calculate downward-rounded RGB thirds from their endpoints. Suitless gray inks calculate upward-rounded fifth steps toward black; neutral base/final roots store one intensity with independent channel overrides. Whiteout interpolates two neutral segments from copied endpoint intensities and keeps independent edited channels. Resolvers preserve complete valid domains and bounds rejection; this rule does not prove callers keep dynamic indices in range."),
    ];
}
