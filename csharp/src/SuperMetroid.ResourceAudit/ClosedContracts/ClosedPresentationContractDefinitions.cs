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
        .. LibraryBackgroundClosedContractDefinitions.All,
        .. RoomLayoutClosedContractDefinitions.All,
        .. RoomLevelStreamClosedContractDefinitions.All,
        .. MotherBrainSheetsClosedContractDefinitions.All,
        .. NativeDisplaySelectorClosedContractDefinitions.All,
        .. EnemyArtworkClosedContractDefinitions.All,
        .. EnemyProjectileArtworkClosedContractDefinitions.All,
        .. EnemyDisplayArtworkClosedContractDefinitions.All,
        .. MessageClosedContractDefinitions.All,
        .. PlmClosedContractDefinitions.All,
        .. PlmActorClosedContractDefinitions.All,
        .. PlmProgressionClosedContractDefinitions.All,
        .. PlmDoorClosedContractDefinitions.All,
        new("SuperMetroid.Core.Hud.Artwork.GameplayHudPresentation", "hud-v2-complete-valid-domain",
            ["ApplyTemplate", "TryApplyIcon", "ApplyEnergy", "ApplyAmmo", "ApplyAutoReserve",
                "ClearAutoReserve", "ToggleItemHighlight", "MinimapCellIndex", "TopRow"],
            [new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayHudPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayHudDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayHudTiles.cs")]),
        new("SuperMetroid.Core.Frontend.FileSelect.Artwork.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Frontend/FileSelect/Artwork/FileSelectPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelect/Artwork/FileSelectHelmetParts.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuHeadingBorderDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuBorderParts.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapSpriteCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuCursorParts.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRoomColorPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainRoomColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainAttackPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRecoveryPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainRoomFlashPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainGlassPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainFinalRoomPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainHealthPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainRoomPaletteProgramDefinitions.cs")]),
    ];
}
