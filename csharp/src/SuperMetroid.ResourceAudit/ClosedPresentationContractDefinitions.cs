namespace SuperMetroid.ResourceAudit;

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
        .. VramDmaPresentationContractDefinitions.All,
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
        new("SuperMetroid.Core.Assets.GameplayHudPresentation", "hud-v2-complete-valid-domain",
            ["ApplyTemplate", "TryApplyIcon", "ApplyEnergy", "ApplyAmmo", "ApplyAutoReserve",
                "ClearAutoReserve", "ToggleItemHighlight", "MinimapCellIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/FileSelectHelmetParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs")]),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainAttackPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRecoveryPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomFlashPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainGlassPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs")]),
    ];
}
