namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed palette-domain closure proofs; no boss AI or palette timing is run.</summary>
internal static class BossColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.BeamPaletteCatalog", "beam-palettes-v1-twelve-selections", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/BeamPaletteCatalog.cs", "908EC1DB16B7D1B862ACFA0C0FC09449D58B2D4505E50DBF4B37F3BEB2AB3938"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "2390C3A34C6DDA0D97FBFA05608C93A2EE1B7F08F35455F2F31A4C2E7C033C7F")],
            "The private-constructor loader requires all twelve named beam selections with sixteen colors each; LoadTo bounds-checks that complete array."),
        new("SuperMetroid.Core.Assets.CeresRidleyColorCatalog", "ceres-ridley-v3-complete-palette-rows",
            ["ApplyStart", "ApplyEyeFade", "ApplyBodyFade", "ApplyHealth", "ApplyAlarm", "ApplyRetreat", "ApplyBaby"],
            [new("csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs", "0F242DF55ADC9876EFE0173F003F9269C1BCD903B5BC6F1CC1DD5835E7F962BD"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "20CB1E292CC94F8BB30D9F237F148789E3E165F58BD116D5259990370C9E17CE")],
            "Load compiles complete start/retreat arrays, sixteen eye/body/alarm rows, three health rows and four baby rows before private construction. Legacy omissions inherit validated stock; Get checks row bounds and Apply uses loaded arrays only."),
        new("SuperMetroid.Core.Assets.CrocomireColorCatalog", "crocomire-v1-complete-fixed-palettes",
            ["ApplyInitial", "ApplyFightBody", "ApplySkeletonArm", "ApplyWallSpikes"],
            [new("csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs", "0E22E4568925289B92BE9F5545F2C76810A727C8003C3793B727AB7BD0F22066"),
             new("csharp/src/SuperMetroid.Core/Game/CrocomirePaletteRomData.cs", "9BA574D8DEC750637921FEC4505D82889D26EC116C4D62FFD09C395F048D0DB2")],
            "Load requires complete fight/wall/projectile/skeleton-arm/spike color arrays before private construction. These four operations transfer fixed loaded arrays, not external identities."),
        new("SuperMetroid.Core.Assets.SporeSpawnColorCatalog", "spore-spawn-v1-complete-scene-palettes",
            ["ResolveSpore", "ResolveHealth", "ResolveDeath"],
            [new("csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs", "A73073C05113BF80CFBE71FD40A9E7303CEA1BE8C60DDDEB2190EB0EC87ECD71"),
             new("csharp/src/SuperMetroid.Core/Game/SporeSpawnColorRomData.cs", "484DBC2370AF21989B53E7AC32BD6282ECCDD07B8F0B9693CA6A171670025EE2")],
            "Private construction requires sixteen-color spore, four health, eight death-sprite and seven death-level/background rows. The layer enum switch rejects invalid values; CheckFrame/Get guard each loaded row/color domain."),
        new("SuperMetroid.Core.Assets.DraygonColorCatalog", "draygon-v1-complete-health-palettes",
            ["ApplyIntro", "ApplyHurt", "ApplyHealthBand"],
            [new("csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs", "8363967BE3CE6455F6DF0E2EC7CA4025204B7D1E7D0C34B9D83998C0660A7ACB"),
             new("csharp/src/SuperMetroid.Core/Game/DraygonColorRomData.cs", "C412E97DE1073FC470C90D9EA56F2BB71BB44F494833FA4E6701C9BB3442E030")],
            "Load requires intro/background/sprite/white-flash arrays and all eight complete health bands before private construction. Health selection rejects odd/out-of-range byte indices; hurt uses only these loaded arrays."),
        new("SuperMetroid.Core.Assets.PhantoonColorCatalog", "phantoon-v1-complete-color-targets",
            ["ResolveHealth", "ResolveFadeOut", "ResolvePowerOn"],
            [new("csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs", "58A768A1998DE907D3E49ACCCB4DE8E001206F116C9D16BC135E00D13F8AB1C6"),
             new("csharp/src/SuperMetroid.Core/Game/PhantoonColorRomData.cs", "797AD8063CCED30E06BE43FD5EC111BFFAD60474331B48EECEF637E3E5F28E28")],
            "Private construction requires all eight sixteen-color health bands, sixteen fade colors and 112 power-on colors. CheckBand/Get guard indices into these complete arrays; no further identity is selected."),
        new("SuperMetroid.Core.Assets.TourianStatueColorCatalog", "tourian-statue-v1-complete-eye-palettes",
            ["ApplyEntrance", "ApplyEye", "ApplyGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs", "5A838AFA770C570075919B8EA2D30ECE071D5AC9AAA84621CB4C1D4D056FD27D"),
             new("csharp/src/SuperMetroid.Core/Game/TourianStatuePaletteRomData.cs", "8B40EBE9A88CA5DF57EE8E5DAE29AD520907B69E38210D28A192A3B53FC1E577")],
            "Load validates complete base/statue/grey arrays and four four-color eye rows before private construction. ApplyEye accepts only even doubled indices 0..6; entrance/grey transfer fixed arrays. Unreviewed raw Resolve methods are excluded."),
    ];
}
