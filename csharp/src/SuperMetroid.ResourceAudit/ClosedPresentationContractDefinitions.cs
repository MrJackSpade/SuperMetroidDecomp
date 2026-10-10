namespace SuperMetroid.ResourceAudit;

/// <summary>A source file whose reviewed code is pinned by its <see cref="SourceFingerprint"/>.</summary>
internal sealed record ReviewedSource(string Path, string Fingerprint);
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
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs", "23B05A19B0E596C803D45B2ACE9BDF66F81FB901A452C155D82ACE9C0752593E"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs", "6AAA9D1CD22AA9A06505C70F5CD8CF6BEA6AB8FA5D88F45608BC8C5A0778B6F3")]),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs", "74FA8CE1DA5BD46BEEBC0AF09723CC7BDDD522184F7576992A605767AB854167"),
             new("csharp/src/SuperMetroid.Core/Assets/FileSelectHelmetParts.cs", "1D35B8CF2CFF286E5A6412C1624859B2172C0A365F61BEE8DCA5764C584E9A7F"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "066962DCF7802C8FC565BD19FC4C0865C1693652A874F5F6894B1D8B9CAB36BF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "C32C723DBBABA4FDEF82FCFFE9AC194861391652F68C05581DC80824F5275032"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "595FA48EE3F54462D44422E1739F0782866FB3673B899E9218B00FEBEE270BB5"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6")]),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs", "44799E3A516E72AD971A3E1470DEE0212628FA4DEC9F472673A637A945F3BAD5"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs", "41A5A4ABD4FA574F3BA6F18C08D760A9CF35483103ABD8FC31787031B1F3E2E6"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainAttackPaintDefinitions.cs", "F8B0DC71C1AE7A7E39257394161274D925406633EAD8C3E230F7216EE638E759"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRecoveryPaintDefinitions.cs", "0EADEC586D637A8F624DD16F915D4AAC463F6620613235D83796A178BC4851DC"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomFlashPaintDefinitions.cs", "19B8DF6476C65840A94B76816B9FFF848E57C9E68149001EDA756EDCBFA779B9"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainGlassPaintDefinitions.cs", "2FD514F241672A1FDEEC749C9EE0AD267A9FAE5F5CFC263261B6EF6C7ADB279C"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "A821A07779A2C9C4065AE75DA484DBE17BEA8D560AE0FB18976177BD4AAB4815"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "E72BF0E51F2E8E5010E9F05BFE62C3E030E21B092601CA46C239D5CA8EAD114A"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "EF4F5F9E2463C12A6C3F37BE9B190186F354C93DF3CED57F1120773C83736612"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs", "A31BF5D82C370A2D9954F1D0A5CEBE6611150CEA102A1C4ADFDF3ED78CA1089F")]),
    ];
}
