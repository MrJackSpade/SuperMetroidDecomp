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
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs",
                "F759177D2372E6F8AC9253A3E7B270B780CBE479AD1EF8A86EEA88D91CC8A143"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs",
                "6AAA9D1CD22AA9A06505C70F5CD8CF6BEA6AB8FA5D88F45608BC8C5A0778B6F3")]),
        new("SuperMetroid.Core.Assets.FileSelectPresentation", "file-select-v1-bounded-fields",
            ["LoadBackground", "Slot", "WriteDigit", "WriteSlotLetter", "CursorPosition",
                "DrawCursor", "DrawHelmet", "DynamicAnchor", "ApplyPatch", "CopyPage", "DrawBorder"],
            [new("csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs", "74FA8CE1DA5BD46BEEBC0AF09723CC7BDDD522184F7576992A605767AB854167"),
             new("csharp/src/SuperMetroid.Core/Assets/FileSelectHelmetParts.cs", "1D35B8CF2CFF286E5A6412C1624859B2172C0A365F61BEE8DCA5764C584E9A7F"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "3C500B0BC39445C8C6EECAE9DD9D34592239BDE1D5255DA05665434C969645DA"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "C32C723DBBABA4FDEF82FCFFE9AC194861391652F68C05581DC80824F5275032"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "3618A0B1A42C5AA6AC26605CAF4FCF2AE0438E98DF4B6C6BDEC2AAA377477F0F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6")]),
        new("SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation", "mother-brain-room-v3-complete-rows",
            ["ApplyFlash", "ApplyFinal", "ApplyPhaseTwoInitial", "ApplyRoomEntry", "ApplyRecoveryLights"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs", "44799E3A516E72AD971A3E1470DEE0212628FA4DEC9F472673A637A945F3BAD5"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomColorRomData.cs", "41A5A4ABD4FA574F3BA6F18C08D760A9CF35483103ABD8FC31787031B1F3E2E6"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainAttackPaintDefinitions.cs", "B42D6ED67F728C420CC0EC1F26AA3CBDB64039286078520E06EB372E59A6FD06"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRecoveryPaintDefinitions.cs", "C55C00F1260C140DF7E70E83B533B0C0D0B59CCDAF3B57B490FC71253688722C"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomFlashPaintDefinitions.cs", "9E65387646AC7D0FA7F6404551D98FC3740D996BA79F7230734C851DB4010CA3"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainGlassPaintDefinitions.cs", "E2DBF571B4FD5EC3EC7E6E48CDD2D51D974DC2BB95635355D64CB1F7810BB739"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "3F20B4045B150989E908FCBC0B00E1CD6662A5AB02645E1B058E29C4BCF82E2E"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "088BE0C52836F6171311561D153692DAFCB9A7ACA711FA42BE1666C81E4C9295"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "B4316A6F8D49A378F36435770B3AFA74338AF7D009D8E0B4E6140E3302655892"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs",
                "A31BF5D82C370A2D9954F1D0A5CEBE6611150CEA102A1C4ADFDF3ED78CA1089F")]),
    ];
}
