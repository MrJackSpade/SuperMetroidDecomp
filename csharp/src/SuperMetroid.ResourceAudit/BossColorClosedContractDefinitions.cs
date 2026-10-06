namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed palette-domain closure proofs; no boss AI or palette timing is run.</summary>
internal static class BossColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.BeamPaletteCatalog", "beam-palettes-v1-twelve-selections", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/BeamPaletteCatalog.cs", "37DE1F3097D9299ACC0C00205B3BF1815411B53425FB58393D43EF1726780C79"),
             new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs", "698EB15B595FC4192181AC1CB301F85602A2F3DAA002076CA90A37CB85A8E232"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "C6FF1C62D59D61DA32F946B3EF98EAF4F3EB7AA2239246EE7F9DADB7408ABEBF"),
             new("csharp/src/SuperMetroid.Core/Assets/SpazerCompositionGeometryDefinitions.cs", "50531F1C89026140C12B131C252FB869BACA5A6EFB931FD619376E20F20C6DCF")],
            "The private-constructor loader requires all twelve named beam selections with sixteen colors each; LoadTo bounds-checks that complete array."),
        new("SuperMetroid.Core.Assets.CeresRidleyColorCatalog", "ceres-ridley-v3-complete-palette-rows",
            ["ApplyStart", "ApplyEyeFade", "ApplyBodyFade", "ApplyHealth", "ApplyAlarm", "ApplyRetreat", "ApplyBaby"],
            [new("csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs", "9D518C166D89DA192F0B055D4CDD199D9F251B8E2E861D79281FC708AB071F26"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresRidleyFadeColorDefinitions.cs", "0D9702B22861DC2994CD31C8A2CCB4CEDA6E44F55C1D7DBE73FE11F8E86F72EB"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresRidleyAlarmColorDefinitions.cs", "CE584950758AB811CD7E8B7143F5EF6CE04AD6F9E503B6B68D8E2FDCCEC13CE7"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "20CB1E292CC94F8BB30D9F237F148789E3E165F58BD116D5259990370C9E17CE")],
            "Load compiles complete start/retreat arrays, sixteen eye/body/alarm rows, three health rows and four baby rows before private construction. Legacy omissions inherit validated stock. Eye/body fade rows resolve exact RGB5 calculations with explicit required endpoints/deviations and independent edits. Alarm rows resolve from required forward colors and exact reflected aliases with independent edits; all Apply paths validate bounds and use installed data only."),
        new("SuperMetroid.Core.Assets.CrocomireColorCatalog", "crocomire-v1-complete-fixed-palettes",
            ["ApplyInitial", "ApplyFightBody", "ApplySkeletonArm", "ApplyWallSpikes"],
            [new("csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs", "0E22E4568925289B92BE9F5545F2C76810A727C8003C3793B727AB7BD0F22066"),
             new("csharp/src/SuperMetroid.Core/Game/CrocomirePaletteRomData.cs", "9BA574D8DEC750637921FEC4505D82889D26EC116C4D62FFD09C395F048D0DB2")],
            "Load requires complete fight/wall/projectile/skeleton-arm/spike color arrays before private construction. These four operations transfer fixed loaded arrays, not external identities."),
        new("SuperMetroid.Core.Assets.SporeSpawnColorCatalog", "spore-spawn-v1-complete-scene-palettes",
            ["ResolveSpore", "ResolveHealth", "ResolveDeath"],
            [new("csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs", "D21513424A2D04B2AC937DC8467096918D3C213280FF70BA79D1A0793EE9F549"),
             new("csharp/src/SuperMetroid.Core/Assets/SporeSpawnDeathColorDefinitions.cs", "152BC71D834EE1CF251AF64DC80CD972D92346E1818D0AA517ADD5E64BA30C13"),
             new("csharp/src/SuperMetroid.Core/Game/SporeSpawnColorRomData.cs", "484DBC2370AF21989B53E7AC32BD6282ECCDD07B8F0B9693CA6A171670025EE2")],
            "Private construction requires sixteen-color spore, four health, eight death-sprite and seven death-level/background rows. The layer enum switch rejects invalid values; CheckFrame/Get guard each loaded row/color domain."),
        new("SuperMetroid.Core.Assets.DraygonColorCatalog", "draygon-v1-complete-health-palettes",
            ["ApplyIntro", "ApplyHurt", "ApplyHealthBand"],
            [new("csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs", "5DC9E26463CE3CFB95CFA2CFA51E6D26E429B2F8B0A51CA2D29EA6672F8C11D6"),
             new("csharp/src/SuperMetroid.Core/Game/DraygonColorRomData.cs", "C412E97DE1073FC470C90D9EA56F2BB71BB44F494833FA4E6701C9BB3442E030")],
            "Load requires intro/background/sprite/white-flash arrays and all eight complete health bands before private construction. Health selection rejects odd/out-of-range byte indices; hurt uses only these loaded arrays."),
        new("SuperMetroid.Core.Assets.PhantoonColorCatalog", "phantoon-v1-complete-color-targets",
            ["ResolveHealth", "ResolveFadeOut", "ResolvePowerOn"],
            [new("csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs", "64EC4A7F8F10DB9E071C6112B5446451F888105028618AD7CF282E60CDEAF441"),
             new("csharp/src/SuperMetroid.Core/Game/PhantoonColorRomData.cs", "797AD8063CCED30E06BE43FD5EC111BFFAD60474331B48EECEF637E3E5F28E28")],
            "Private construction requires all eight sixteen-color health bands, sixteen fade colors and 112 power-on colors. Bounded resolvers select calculated black/tint targets, required endpoint/deviation inputs or independently supplied overrides; power-on colors remain a complete installed array. No further identity is selected."),
        new("SuperMetroid.Core.Assets.TourianStatueColorCatalog", "tourian-statue-v1-complete-eye-palettes",
            ["ApplyEntrance", "ApplyEye", "ApplyGrey"],
            [new("csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs", "7FDE8ED04F0B0ACAD46756A1F3FA5739E023D347A1CD08025AA7AC6A2F837FC7"),
             new("csharp/src/SuperMetroid.Core/Game/TourianStatuePaletteRomData.cs", "8B40EBE9A88CA5DF57EE8E5DAE29AD520907B69E38210D28A192A3B53FC1E577")],
            "Load validates complete base/statue/grey arrays and four four-color eye rows before private construction. ApplyEye accepts only even doubled indices 0..6; entrance/grey transfer fixed arrays. Unreviewed raw Resolve methods are excluded."),
    ];
}
