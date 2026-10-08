namespace SuperMetroid.ResourceAudit;

internal sealed record ReviewedSource(string Path, string Sha256);
internal sealed record ClosedPresentationContract(string Type, string Rule, string[] Methods,
    ReviewedSource[] Sources);

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
                "550027E3BFCC6F00A3189D4E122510BF78831E0A898E303774DE04DC1E383A0B"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs",
                "0E2B93E2A12A5215127DAC454FE76248F8D05787EA7E88D7E9622B1FE1C7D2C5")]),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs", "7F21AB1D2D58188EF2A2ED7953C1733665A9DCBFAEE08F3FFDE73F7FD3B755E2"),
             new("csharp/src/SuperMetroid.Core/Assets/FileSelectHelmetParts.cs", "E8CBB300A98ED35D2DDB5D927C8D3A3BE8C7E72FD5EE47FA886BA6A1AE02FC98"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "A492AC3C7692DA38E0E838CAB60DC605DDC8E875DBE098475EBAC530C0E47372"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "6B79556FD47098253A8A977E398C98A1908D01A7DD4E9E5916A38634A33E0080"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "155C4933434355E65FA4AFD066701B8720222BB5F0B38020FC170048BBDC0DEE"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "92CFE18B185C1526FACE125CF345944D1D878525935BE219DC21EE19838E3E9E"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "718108CB3359B183DF55EBB1F4C86D41E0858FC5DFE1AF6A01FC0612E69AD0FF")]),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs", "9858BA667DD808C94E9984AE34F086C5411FE58CA24B9B52CA0E025C0DAD4619"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs", "803A616ED9BCFEA112F306393FBB10FC56FBB2BA328923A73DBB9B60DB2C4971"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainAttackPaintDefinitions.cs", "6A30A7BD6A6D487987ED6F3372ED19E301AF3F9FF2EAEC4F37CD43D17830610F"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRecoveryPaintDefinitions.cs", "182FCC0906C39FD23DB0C005AA8A25B85D841FB9D7A02ADC0B906C9AA389C928"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomFlashPaintDefinitions.cs", "B41FFDAFEDA73C02387816FB9FABD5B5E61983C859ABC3BAFB4511FAC6E6B0F1"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainGlassPaintDefinitions.cs", "5CF9DA83AD5DA53A6D81F35207F55BE6878342439F937D65588E3A1C9C482B1C"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "E9302C2C9C8733A7BF410543E4CBECCFA6F5806C88C616761274D7C3FC6357E0"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "05E148A6572B5F7FC629794FC4F0853D9E73DC2D861C721B4843CBB7FF3169E4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "CED903B0B3BD50BFD557B00AF84843408D5E3476F04BAA309BE6C043B101C37D"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs",
                "42D56552264EEE40B3D5DBAE099E32F071214D5D393DAD84436C725C5C99AE46")]),
    ];
}
