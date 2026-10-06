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
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs", "7F21AB1D2D58188EF2A2ED7953C1733665A9DCBFAEE08F3FFDE73F7FD3B755E2"),
             new("csharp/src/SuperMetroid.Core/Assets/FileSelectHelmetParts.cs", "E8CBB300A98ED35D2DDB5D927C8D3A3BE8C7E72FD5EE47FA886BA6A1AE02FC98"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "A492AC3C7692DA38E0E838CAB60DC605DDC8E875DBE098475EBAC530C0E47372"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "6B79556FD47098253A8A977E398C98A1908D01A7DD4E9E5916A38634A33E0080"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "6FDADA7BC65E29CCCA60F6D9301DD5B6020202553A025A2994AADA8BE497DA28"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "92CFE18B185C1526FACE125CF345944D1D878525935BE219DC21EE19838E3E9E"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "718108CB3359B183DF55EBB1F4C86D41E0858FC5DFE1AF6A01FC0612E69AD0FF")],
            "Load is the sole private-constructor path and requires exact page, patch, sprite, " +
            "border and dynamic-anchor sets plus complete digit/letter/slot/cursor arrays. " +
            "Reviewed array selectors reject invalid indices; generated cursor/helmet names are bounded. " +
            "Named patch/anchor/page/border calls additionally require a compiler-resolved finite " +
            "installed-key set through constants or closed source flow; arbitrary strings remain unresolved."),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs", "9858BA667DD808C94E9984AE34F086C5411FE58CA24B9B52CA0E025C0DAD4619"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs", "8A38E59CBB3FD1EAA6FFEB2CC26906BBD26B60BA284B3D49A8346ED7DB16CC85"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainAttackPaintDefinitions.cs", "6A30A7BD6A6D487987ED6F3372ED19E301AF3F9FF2EAEC4F37CD43D17830610F"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRecoveryPaintDefinitions.cs", "182FCC0906C39FD23DB0C005AA8A25B85D841FB9D7A02ADC0B906C9AA389C928"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomFlashPaintDefinitions.cs", "B41FFDAFEDA73C02387816FB9FABD5B5E61983C859ABC3BAFB4511FAC6E6B0F1"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainGlassPaintDefinitions.cs", "5CF9DA83AD5DA53A6D81F35207F55BE6878342439F937D65588E3A1C9C482B1C"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "E9302C2C9C8733A7BF410543E4CBECCFA6F5806C88C616761274D7C3FC6357E0"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs",
                "B498159F68527B4428D6AADFCC65881BD4F6820D0C6266AD22830F451D17A39B")],
            "The validated loader installs all fourteen aligned flash rows, seven recovery-light " +
            "rows and the fixed final/phase-two/room-entry arrays before private construction. " +
            "Legacy omissions inherit only from validated stock. Flash alignment/range and recovery " +
            "indices are guarded; fixed operations perform no external identity lookup."),
    ];
}
