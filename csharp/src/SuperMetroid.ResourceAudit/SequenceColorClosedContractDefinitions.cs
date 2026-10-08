namespace SuperMetroid.ResourceAudit;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["BeamColorWord", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "19FAC527E2A486E0F4270384D2315AAD2F8A5F1DCF18AEE69EBB1202F1769162"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs", "9E2791CED6D9D905248F39DFF0D5E486E297DE8BCB2EF526B9D644A26DF4CAF8"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "D82DF9109B8F46520C845F6F51058990DBD365360CF70C07D6A45BAAE8AB2D61"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs", "8FF000D50D77216696DF8B2BB3D75D90BBC90B697FEEEFAAFEAA07325CE6135E"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDrainedPaletteRomData.cs", "062F4B0C1DA32E19C130228C2F62873BEEEDFFE7792192402D4CAC5DBB9BA570"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainFakeDeathPaletteRomData.cs", "C3CC3D0125D864E07ACAFF817517A34519B2B9011261B566BECA227A3EBD736C"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs", "20E95A0BCE5A2E07061C9929E04B040F2486C049CF3529733CA74D7127B9182A")]),
        new("SuperMetroid.Core.Assets.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs", "2FDE4827B0B7511D23CC55D2C73C32A3403A4A3CD6A0D66F9889C768E9902902"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "D82DF9109B8F46520C845F6F51058990DBD365360CF70C07D6A45BAAE8AB2D61"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "19FAC527E2A486E0F4270384D2315AAD2F8A5F1DCF18AEE69EBB1202F1769162"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs", "9E2791CED6D9D905248F39DFF0D5E486E297DE8BCB2EF526B9D644A26DF4CAF8"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDeathRomData.cs", "081EFAD19AF9C9D1B9B39DA09362624FB6481417227CA5FDCDC5BD5E9722A96B"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainExplodedDoorPaintDefinitions.cs", "B7F02AB930B4EF3036C5665F3C76E5F03236C7983047D68118645A40DAA67708"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "E9302C2C9C8733A7BF410543E4CBECCFA6F5806C88C616761274D7C3FC6357E0")]),
        new("SuperMetroid.Core.Assets.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs", "94968A5E262587D4F82908624F6B39C0654B823B37B1CB6F6BE596C0DA33CDA4"),
             new("csharp/src/SuperMetroid.Core/Game/ChozoAndTubeColorRomData.cs", "6A9A2B10A9A97A66584EC7DD28C562D687E7E1CC45376990A57D2DCB1D4B452B"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStatuePaintDefinitions.cs", "B424DA550D26CAA31E64F0192D9D01115BB55E374DF5C72DDE28112D75A775F5")]),
        new("SuperMetroid.Core.Assets.GameplayBasePaletteCatalog", "gameplay-base-v1-complete-cgram-image",
            ["LoadInitial", "LoadCommonSprites", "LoadEnemyProjectileSprites"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs", "9B46366001ABA048A5FF5068460255592A282FB13552AEE0BDE4C7DF42CECFE9"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesCgram.cs", "04CB6C5C68A00A0527005221D8C8F40706F567368DF6E4ADB01333DC22CD1E90"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesPpuLayout.cs", "A880E1FC838925490AEEFBC62E19495CA9311EF9D329C39BB234EC78109C2E4E")]),
        new("SuperMetroid.Core.Assets.SamusDeathPaletteArtworkCatalog", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusDeathPaletteArtworkCatalog.cs", "680EFE2E59171A969FC2B9960B4BED8137C36298AD4170806AAB135908E24750"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "4CBE656DC00C6CF33257B12449042328AB3C97DA6DD1901816F82B138BBC7C23"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs", "CADFA0A1E5CC12FCF1EC6868D0EF7FEEAFF1DC1816046CF0C90F7FCACBA65325"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "452B184D6FC9F55826EBDF8AB90EDA286A900B50B4CA0633C79D065B063E6F9C")]),
    ];
}
