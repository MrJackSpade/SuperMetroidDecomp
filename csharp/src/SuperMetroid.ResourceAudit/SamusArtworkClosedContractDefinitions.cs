namespace SuperMetroid.ResourceAudit;

/// <summary>Complete placement/composition domains, separate from editable DMA frame-selector closure.</summary>
internal static class SamusArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-placement-offsets",
            ["GraphicsYOffset", "TryLandingYOffset", "TryPostureYOffset", "TryDrainedYOffset"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "363B67BAE7EB8A9E1080B8189072395D5DB6DE10B8B5BB44C8B9E5C6C3B4AB74"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.ContentIdentity.cs", "A9BCEECECD5218005E8DCB9880BB5B4E85474BF11D6671366082FBDABB63B8B8"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "92B942C3417DA086E5CDCF45B2594A28B339E96AF222EFBC5035DCB6DBEBB8DE")],
            "Public construction requires and clones all pose, landing-adjacent-byte, posture and drained offset arrays. Try methods bounds-check and validly return false outside those arrays. This proves only placement-table coverage, not Frame/GetDefinition/DefinitionAt selector ownership, animation or correct pixel origins."),
        new("SuperMetroid.Core.Assets.SamusSpritemapArtworkCatalog", "samus-complete-indexed-composition-membership",
            ["TopBase", "BottomBase", "TryGet"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "E33B6FFC34043ADF05D7272941306262AFA90427378916D1B7CF9F5F3500F48B"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "363B67BAE7EB8A9E1080B8189072395D5DB6DE10B8B5BB44C8B9E5C6C3B4AB74")],
            "Public construction requires 253 upper/lower pose bases and all 2096 indexed pointers, validating every nonzero pointer against its independently copied composition. Native zero pointers validly return false. Exposed part arrays do not invalidate pointer membership, but post-publication pixel integrity, mutable-memory fallback and caller animation indices are not certified."),
        new("SuperMetroid.Core.Assets.SamusAtmosphericArtworkCatalog", "samus-complete-direct-atmospheric-membership", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs", "1CBF7C47C9AB585E7BE89A3E5A5328C73B0F53223445765835CAB4443264CFEB"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "F08777915DBEA264FF7589BC67049A4F2A17321A5DDAF4A09F615BB8D1A07D05")],
            "Public construction requires both four-word direct atmospheric lists and clones their arrays. Unsupported types or frames return false without demanding absent artwork. This proves only the direct-list membership query, not mutable-memory fallback, timing, uploaded characters or pixels."),
        new("SuperMetroid.Core.Assets.SamusArmCannonArtworkCatalog", "samus-complete-cannon-placement-and-tile-membership",
            ["PoseDrawingData", "ReadDrawingByte", "SpriteAttributes", "TileSource", "TryResolveTile"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "5FC3060A58A9FB345A62521ED7A0DD505D7672A4EBBC8C8A0AFDED67A6268210"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "363B67BAE7EB8A9E1080B8189072395D5DB6DE10B8B5BB44C8B9E5C6C3B4AB74"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "92B942C3417DA086E5CDCF45B2594A28B339E96AF222EFBC5035DCB6DBEBB8DE"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs", "0C2CD85F446A356CF2A0E226E7F9D45C64F23C118A58097058E2B33152F97512")],
            "Public Load validates all pose descriptors, drawing bytes, ten attribute/direction rows and four tile selectors per direction, then compiles all twelve cover tiles. Core access to FromPlacement outside the provider, or to mutable TileSourcePointers, revokes this production-loader proof. Unsupported tile source/length queries validly return false. Arbitrary external factory instances, host installation, firing and muzzle position are not certified."),
    ];
}
