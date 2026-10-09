namespace SuperMetroid.ResourceAudit;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["BeamColorWord", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "A3CCC6838C89D82817EA53FBF488EDED4E6F7A182161B4C8500070050E1B422F"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs", "5179BCC48538C7802F1D2A628353607634B27BBB631878F85CC89542F72DD99B"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "8C0B2C704A1E7C75FD2C5F26A64A8F1EEFB68241EF75541A13D9D4BB07928A2A"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "B4316A6F8D49A378F36435770B3AFA74338AF7D009D8E0B4E6140E3302655892"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "3896F2463849CF202066DBA12D319B7C5E079DAA991A5CEB48916CD7F39FCEC7"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs", "5093BEDF978F606D0E670F90DCFF088D0926FFE306C7A7168A72F2BB216403AA"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDrainedPaletteRomData.cs", "F23F5D465EBB755D1CD26BE8E82E7A2497FB49BAE7D8C998935641705BA7E38D"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainFakeDeathPaletteRomData.cs", "F1F55DBEA104BE97D296D42F1A302230CC1C2CBF41E5A25DC9EAAAA606811559"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs", "666FA41A879786E9A6D4A1244C24E9632C834CBFF1D12D06D5A8A8401CA8C91C")]),
        new("SuperMetroid.Core.Assets.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs", "30B54771EB20BF0C8378A0E19852457C11A63216F17BAB4205627D9C1A0EFE1F"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "8C0B2C704A1E7C75FD2C5F26A64A8F1EEFB68241EF75541A13D9D4BB07928A2A"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "B4316A6F8D49A378F36435770B3AFA74338AF7D009D8E0B4E6140E3302655892"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "3896F2463849CF202066DBA12D319B7C5E079DAA991A5CEB48916CD7F39FCEC7"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "A3CCC6838C89D82817EA53FBF488EDED4E6F7A182161B4C8500070050E1B422F"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs", "5179BCC48538C7802F1D2A628353607634B27BBB631878F85CC89542F72DD99B"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDeathRomData.cs", "2F51242DC5ED4F25EF218F1B29745A0AD71E6350B145FC821AD8DFC2A6BDC7D9"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainExplodedDoorPaintDefinitions.cs", "C284BF39D367CCC459901780F248E0A1D6A9E5A12CCB77E02B3B68974F0D1953"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "3F20B4045B150989E908FCBC0B00E1CD6662A5AB02645E1B058E29C4BCF82E2E")]),
        new("SuperMetroid.Core.Assets.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs", "59BA6ACCCF7D49233C08EEC9A42753E8676BCF3B2DB232C9F266008655390DA3"),
             new("csharp/src/SuperMetroid.Core/Game/ChozoAndTubeColorRomData.cs", "4BFF71FBD5A00111871F0743A814DF4D9324A3CB2F9DB54D6CBF7E3C2EC28BA5"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStatuePaintDefinitions.cs", "8A03B91BBA695C9CE386C1F17076C369DA265DF11F83EC5BEFE7A251AE398C68")]),
        new("SuperMetroid.Core.Assets.GameplayBasePaletteCatalog", "gameplay-base-v1-complete-cgram-image",
            ["LoadInitial", "LoadCommonSprites", "LoadEnemyProjectileSprites"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs", "29DE00974F156F7C63D142203DC771D5A7376F1CF8739D30132285F9FD9E7B8C"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesCgram.cs", "6D0F4A9104ED8A9B2FAF3061927715316D3B2641FAFB696AEC99C8AD28C28029"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesPpuLayout.cs", "C6753DAA77F9809E6D599040E9E4F13561458352D11DAA0D7EC89B0BFBE3A6C2")]),
        new("SuperMetroid.Core.Assets.SamusDeathPaletteArtworkCatalog", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusDeathPaletteArtworkCatalog.cs", "2FE54C54236063D4EEB59A2AF932C8A23796B97AD1EEE3BCF494440F7C2D9895"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "E22A292E690FB7D6BE8D1CCAB546358F1E94EBEBD175D9AD4094538A3DD8C2CE"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs", "063EA177518DE0CF5C8EA5BD064C2C20E19E1A9D796A632384DDC7F6113EAE2E"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304")]),
    ];
}
