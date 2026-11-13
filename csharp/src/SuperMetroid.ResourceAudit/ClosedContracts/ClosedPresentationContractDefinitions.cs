using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.ResourceAudit.ClosedContracts.Frontend;
using SuperMetroid.ResourceAudit.ClosedContracts.Rooms;
using SuperMetroid.ResourceAudit.ClosedContracts.Samus;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>
/// Source-reviewed closure proofs, not an allowlist of failing calls. Only these
/// operations qualify, and every declaration of a provider must be among its listed
/// sources. See CLOSED-PROVIDER-CONTRACTS.md.
/// </summary>
internal static class ClosedPresentationContractDefinitions
{
    /// <summary>Combines the reviewed closed artwork providers used to classify
    /// owned presentation calls across the repository.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        .. MenuClosedPresentationContractDefinitions.All,
        .. BossColorClosedContractDefinitions.All,
        .. SequenceColorClosedContractDefinitions.All,
        .. SamusColorClosedContractDefinitions.All,
        .. RemainingEnemyColorClosedContractDefinitions.All,
        .. TextAndMapClosedContractDefinitions.All,
        .. PauseClosedContractDefinitions.All,
        .. CinematicClosedContractDefinitions.All,
        .. ProjectileClosedContractDefinitions.All,
        .. SamusArtworkClosedContractDefinitions.All,
        .. SamusBodyTransferClosedContractDefinitions.All,
        .. InterfacePresentationContractDefinitions.All,
        .. BackgroundTransferClosedContractDefinitions.All,
        .. RoomArtworkClosedContractDefinitions.All,
        .. AreaMapClosedContractDefinitions.All,
        .. NativeArtworkAliasContractDefinitions.All,
        .. RoomRevealClosedContractDefinitions.All,
        .. RoomLayoutClosedContractDefinitions.All,
        .. RoomLevelStreamClosedContractDefinitions.All,
        .. MotherBrainSheetsClosedContractDefinitions.All,
        .. NativeDisplaySelectorClosedContractDefinitions.All,
        .. EnemyArtworkClosedContractDefinitions.All,
        .. EnemyProjectileArtworkClosedContractDefinitions.All,
        .. EnemyDisplayArtworkClosedContractDefinitions.All,
        .. MessageClosedContractDefinitions.All,
        .. RoomPictureClosedContractDefinitions.All,
        .. SpawnedObjectSheetClosedContractDefinitions.All,
        .. RoomObjectFrameClosedContractDefinitions.All,
        new("SuperMetroid.Core.Hud.Artwork.GameplayHudPresentation", "hud-v2-complete-valid-domain",
            ["Icon", "EnergyTank", "EnergyCount", "Ammo", "AutoReserve", "MinimapCell"],
            [new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayHudPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayHudPresentationParts.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayHudDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/HudIconImage.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayHudArtwork.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayHudIcon.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayHudPresentationFile.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayHudPresentationExtractor.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayHudLayoutFile.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayHudLayoutDocument.cs")]),
        new("SuperMetroid.Core.Frontend.FileSelect.Artwork.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["Slot", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Frontend/FileSelect/Artwork/FileSelectPresentation.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Frontend/Map/Artwork/MapSpriteCompositions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Frontend/Menus/MenuMissileSpritemaps.cs")]),
    ];
}
