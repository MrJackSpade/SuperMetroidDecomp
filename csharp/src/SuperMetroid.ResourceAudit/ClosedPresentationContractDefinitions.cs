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
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs", "E77C3C85AE010E78DFB8FFF5B7DE5AA3B664DEFB22BDEE2E7684DB0D35197680"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs", "4D560A1B25F2069994A330C3003F75A83F64AED57703A7CCA1E2D984F3071549")]),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs", "E89ECCEFB1222D8967FDBE7119E474B70350997A9D50E39E77C31F4288CB5F48"),
             new("csharp/src/SuperMetroid.Core/Assets/FileSelectHelmetParts.cs", "1D35B8CF2CFF286E5A6412C1624859B2172C0A365F61BEE8DCA5764C584E9A7F"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "066962DCF7802C8FC565BD19FC4C0865C1693652A874F5F6894B1D8B9CAB36BF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "C32C723DBBABA4FDEF82FCFFE9AC194861391652F68C05581DC80824F5275032"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "72753125C34DD2B35620C322E0F6406386172EE65A50E0D4A2878D761E7B5CB8"),
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
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "90B3B500714734717CC515A558FC4A16BB382AC45300BA5AF62FD5B8FB963084"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "EF4F5F9E2463C12A6C3F37BE9B190186F354C93DF3CED57F1120773C83736612"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs", "A31BF5D82C370A2D9954F1D0A5CEBE6611150CEA102A1C4ADFDF3ED78CA1089F")]),
    ];
}
