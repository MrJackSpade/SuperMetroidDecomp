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
        new("SuperMetroid.Core.Assets.GameplayHudPresentation", "hud-v2-complete-valid-domain",
            ["ApplyTemplate", "TryApplyIcon", "ApplyEnergy", "ApplyAmmo", "ApplyAutoReserve",
                "ClearAutoReserve", "ToggleItemHighlight", "MinimapCellIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs",
                "D058D17F344E661769A687F91D58C7A2DD3134E643785428A262AA7152A486D2"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs",
                "ED88E0937A180EB6AE070D1659668C6E7E6E833EB9C615E105AB8AA4363198AC")],
            "The sole private constructor compiles every HUD field: two ten-digit arrays, " +
            "all five named icons, fourteen tank anchors, six AUTO cells and the fixed template. " +
            "These operations select only that validated domain; digit arithmetic is modulo ten, " +
            "tank iteration is clamped, and item/coordinate selectors are range-guarded. " +
            "MinimapCellIndex computes layout, not a resource identity."),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs",
                "154316DA271943BF908297D5CC5D09483A070C1056027495C73A37498664DA45")],
            "Load is the sole private-constructor path and requires exact page, patch, sprite, " +
            "border and dynamic-anchor sets plus complete digit/letter/slot/cursor arrays. " +
            "Reviewed array selectors reject invalid indices; generated cursor/helmet names are bounded. " +
            "Named patch/anchor calls additionally require a compiler-resolved installed key. " +
            "Dynamic page and border selection are deliberately not covered."),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs",
                "67630C828581AB534E1F6EBF0E8145762623EEE80F26B534157EB72FEC780AE8"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs",
                "1C721E9C962B99671899993D8228020C90266EB796466758A0FD6BF6351E424D"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs",
                "68E027B6CA7DCA4C6357729EE8C73276E303D9E94AD934481442F59D4026E1E9")],
            "The validated loader installs all fourteen aligned flash rows, seven recovery-light " +
            "rows and the fixed final/phase-two/room-entry arrays before private construction. " +
            "Legacy omissions inherit only from validated stock. Flash alignment/range and recovery " +
            "indices are guarded; fixed operations perform no external identity lookup."),
    ];
}
