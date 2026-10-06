namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete projectile/rope visual domains; no firing, motion, damage or sound is exercised.</summary>
internal static class ProjectileClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.BeamTileCatalog", "beam-complete-sheet-selection", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "50424F1949ED3D0DD46BD79E4B6840E688E5C92696CBD5E3B0B732E4F24F1D87"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "2390C3A34C6DDA0D97FBFA05608C93A2EE1B7F08F35455F2F31A4C2E7C033C7F"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "73DCE6788B0BB9B1549CF04466AE14CB8AABA3AD9FCC2738E188ABBA2F2F8D44")],
            "Both constructor paths require all twelve ordinary beam sheets and the separate Chainsaw and SpaceTime sheets; the precompiled path rejects nulls and clones its input array. Resolve guards the contiguous ordinary beam asset range and admits only the appended Chainsaw and SpaceTime IDs beyond it. Optional palette providers, transfer timing and selected beam physics are not certified."),
        new("SuperMetroid.Core.Assets.ChargeFlarePlacementCatalog", "flare-complete-standing-running-offsets", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementCatalog.cs", "561687A6F026FB59D25DE33CB0121ACE45CB6CC00F3FA0DFCBD479E66B653459"),
             new("csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementDefinitions.cs", "4B2475E4CF89A5499187BDE56759F80624226C8F51FCB766829B46AFEA483F6D")],
            "Private construction requires both complete sixteen-direction rows of immutable offset records. Resolve checks direction before selecting standing/running. This includes bounded low-nibble overread directions, not just named aim directions. Actual muzzle placement is not certified."),
        new("SuperMetroid.Core.Assets.ChargeFlareSpriteCatalog", "flare-complete-private-composition-set", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteCatalog.cs", "7D0D8F666C0F195D4EB2EAA4FC3B4731FF6D283ED7980A89691E261F8BCEA427"),
             new("csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs", "B6E6668A4579F83232B4D98EE5DD3A9C12FF0644BCFBAD80CB9D5106523C508D"),
             new("csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs", "913EEEF871A5E8396356377900A90DAFD951290CAFB6E2D9F5E137A82CF78AAD")],
            "The sole private wrapper construction loads every one of the 28 required flare compositions through the shared exact-set loader. All 54 selector entries resolve within that set; Draw bounds-checks the selector. The partial shared catalog remains private. Cadence, positioning and projectile mechanics are not certified."),
        new("SuperMetroid.Core.Assets.ProjectileTrailCatalog", "trail-complete-timed-appearances-and-retained-start", ["Resolve", "ResolveCurrent"],
            [new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailCatalog.cs", "8C1BF36F6377EFC5E85D3A5BCADD664E4489E9D94F27ACED6A10689A3CCE5492"),
             new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailVisualDefinitions.cs", "7D634DDFAF877FA878B7428D66AFDEB1D35A2AE85C73718EB2A917F338940CA5"),
             new("csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs", "809E6328A92274EF0857EA4DC674D75C4EA1FB98E4866F07DDFBB243EB1543CD")],
            "Private construction requires every sparse timed trail appearance. ResolveCurrent either preserves retained native attributes at one of five unconsumed list starts or selects the installed frame four bytes before the next instruction. Those distinct input domains are checked. Optional tile installation and trail clocks are not certified."),
        new("SuperMetroid.Core.Assets.ProjectileFrameBindingCatalog", "projectile-complete-timed-bindings", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs", "1CB4225D60B8CAF9C5313E7766ECADCFF5D440768A5EA097B835CB05257E6AE4"),
             new("csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs", "1F147EAA9C200A009926A4F9EBCE21D2B40D589AC63BB913197849EF959C87C2"),
             new("csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs", "FB4C6216393398B4A3C16600A72EF44DB9605F82C53B91A2AEAC27A30D30F365")],
            "Private construction requires bindings for every one of the 805 sparse compiled timed-record owners and validates every target against legal sprite IDs. External Core access to the array-backed TimedRecordPointers property revokes this proof. Target legality does not prove the chosen animation is correct or that a host installed the right sprite instance."),
        new("SuperMetroid.Core.Assets.GrappleSpriteCatalog", "grapple-complete-endpoint-and-segments", ["Segment"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs", "498716ED3959ACE16B883187EAA9600750C13B81E26A139CFCD7757C8A351AC5"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs", "8AD99F0497283332E9D13D1847B8E34B7BB5CC66C69044E75D13910F615F3C52")],
            "Private construction compiles the endpoint and all four timed segment appearances. Segment guards the four indices; colors, rope geometry, sound and connection physics are not certified."),
        new("SuperMetroid.Core.Assets.GrappleSwingFrameCatalog", "grapple-complete-byte-angle-display-map", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "48F98641513B038D3B4A73EBD5D6F46F17DE8F22B74DA5D91B7B883CFD758FDE")],
            "Private construction requires all 256 angle entries with display frame values zero through 31, compiled independently. Every byte angle selects a loaded entry. Body artwork installation and wall/swing/jump physics are not certified."),
        new("SuperMetroid.Core.Assets.ProjectileSpriteCatalog", "projectile-production-complete-compositions", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs", "913EEEF871A5E8396356377900A90DAFD951290CAFB6E2D9F5E137A82CF78AAD"),
             new("csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs", "FB4C6216393398B4A3C16600A72EF44DB9605F82C53B91A2AEAC27A30D30F365"),
             new("csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteCatalog.cs", "7D0D8F666C0F195D4EB2EAA4FC3B4731FF6D283ED7980A89691E261F8BCEA427")],
            "Production Load requires all 417 sparse projectile compositions. The alternate partial LoadFrames is confined to the source-guarded flare wrapper's private field and the provider itself; any other Core reference revokes this production-consumer proof. No assertion about arbitrary partial instances, host installation, animation or pixels is made."),
    ];
}
