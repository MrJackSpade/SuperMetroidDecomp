namespace SuperMetroid.ResourceAudit;

internal sealed record ReviewedSource(string Path, string Sha256);
internal sealed record ClosedPresentationContract(string Type, string Rule, string[] Methods,
    ReviewedSource[] Sources, string Reason);

/// <summary>
/// Source-reviewed closure proofs, not an allowlist of failing calls. Only these
/// operations qualify, and any change to a provider or its definition set revokes
/// the proof. See CLOSED-PROVIDER-CONTRACTS.md before updating a fingerprint.
/// </summary>
internal static class ClosedPresentationContractDefinitions
{
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
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs",
                "3EAF5149EF17F94080F0C6C96FFB4EF008A67ACF110CA9D4FB3BA8229AE76347"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs",
                "ED88E0937A180EB6AE070D1659668C6E7E6E833EB9C615E105AB8AA4363198AC")],
            "The sole private constructor compiles every HUD field: two ten-digit arrays, " +
            "all five named icons, fourteen tank anchors, six AUTO cells and the fixed template. " +
            "These operations select only that validated domain; digit arithmetic is modulo ten, " +
            "tank iteration is clamped, and item/coordinate selectors are range-guarded. " +
            "MinimapCellIndex computes layout, not a resource identity."),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs",
                "76F29F9DF4CE8062EFCF77DFE8447D4CBC0355447CEFCC5F81195FD529044A04")],
            "Load is the sole private-constructor path and requires exact page, patch, sprite, " +
            "border and dynamic-anchor sets plus complete digit/letter/slot/cursor arrays. " +
            "Reviewed array selectors reject invalid indices; generated cursor/helmet names are bounded. " +
            "Named patch/anchor/page/border calls additionally require a compiler-resolved finite " +
            "installed-key set through constants or closed source flow; arbitrary strings remain unresolved."),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs",
                "530305da8f3ea987c4b667ead93a8cb891e7e9c82d92b7e02d11caf57e02bb19"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs",
                "1C721E9C962B99671899993D8228020C90266EB796466758A0FD6BF6351E424D"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs",
                "B7D59A301621835F4C4708765DBF3458C6BF08B429A459F65A978D07EDA92E7A")],
            "The validated loader installs all fourteen aligned flash rows, seven recovery-light " +
            "rows and the fixed final/phase-two/room-entry arrays before private construction. " +
            "Legacy omissions inherit only from validated stock. Flash alignment/range and recovery " +
            "indices are guarded; fixed operations perform no external identity lookup."),
    ];
}
