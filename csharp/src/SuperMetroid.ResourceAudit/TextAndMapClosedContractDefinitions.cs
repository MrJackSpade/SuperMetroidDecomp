namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed text and map loaders with complete finite resource domains, not cinematic or navigation tests.</summary>
internal static class TextAndMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EscapeTypewriterPresentation", "escape-text-complete-program-membership", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs", "28C345FA09AA1975750B4B3CCDEE9195C6FEBDB0D777F738D4F913F0CC8F9C13"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterDefinitions.cs", "E81816E00AB6B6844553D5797955DBD1156A8E0E7D744BEE6EDC646B8B06813A")]),
        new("SuperMetroid.Core.Assets.IntroNarrationPresentation", "narration-complete-compiled-pages", ["GetLines", "Compile"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs", "A449A81F05E2FD96D57A25F8C025B967F96ADE1549B75B91A2B1CD17B85EB62B"),
             new("csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs", "B834E985A30B202CBDB52E33423DE4186727DA9B381BF7A0465DCB6DA3F42E2D")]),
        new("SuperMetroid.Core.Assets.EndingTextPresentation", "ending-text-complete-panels-and-sequences", ["Compile", "BuildResultPanel", "BuildCopyrightPanel"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingTextLayoutDefinitions.cs", "074F2620779C3BBC8BFD6E8FE4F6E6139BFC1CE133B63F1029173D5B3544FC38"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs", "25CDF798FAF98261DBE7D50D707B75FC466F30F595D15CAF8C4EC938D5F75E91"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextDefinitions.cs", "D37D71282946543594B6C1FF3881E222D5A2C9F5C085833F53F43BE6C53FCE52")]),
        new("SuperMetroid.Core.Assets.CreditsPresentation", "credits-loaded-complete-row-domain", ["GetRow"],
            [new("csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs", "0595E22D2175E47BEE610CC7E3C598E1A4F47CD5689A1162260EBF9970F5705E"),
             new("csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs", "D72D6569C182EA810D4650929AD87D9E382B0769B2073BEC649D575B0B48945D"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextDefinitions.cs", "D37D71282946543594B6C1FF3881E222D5A2C9F5C085833F53F43BE6C53FCE52")]),
        new("SuperMetroid.Core.Assets.MapSpriteCatalog", "map-sprite-complete-named-compositions", ["Draw", "LoadArtworkTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "F60AD7B4167887D4942D3C5FEA672275338B39D82725CBFBF90A6EF70E589619"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerGeometry.cs", "D6F3921874E7FE5D1E2A6A96D1A9CD2E5C9DF06E3517419E18DE016B2F26ED0A"),
             new("csharp/src/SuperMetroid.Core/Assets/WorldMapLabelComposition.cs", "6076CB1B5DABAC7C0B75B9FF413305E10755E7F32C2CE917A0062DE463C54BA5"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuSmallFontArtwork.cs", "E7366DAF5100AC184A142A8DB25121215ED1804480C6BC963BC315145FAB1CCA"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuLargeFontArtwork.cs", "A4D7EE7AD770EA6F73106177EE920009D4B4CF8D58E7868CBF2602EE688BC156"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCompactLetteringArtwork.cs", "6A04CABAAB759493AE80FC3E9B2F36D93DB2AED16B42768B5437F0CFA9C60C42"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuPanelTileArtwork.cs", "55CEB402145A6F9EA26A5AE3DDC3572715FD115BB8F102B45CFE6DFD45F268F1"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderButtonArtwork.cs", "0DD5C5B7109B001489FEF5BB4913D9481880BCFB170EAB9502CEC526EF1957DF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderHighlightArtwork.cs", "FCB9A9AA2C00BE679F791850025ACA7F14E900A686F4B5D9C9365C498C307392"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerTileArtwork.cs", "77CB8BBF24EB16BB5E67B741DAAADF66D41903AAA74652F8928E1BA252CFD3BA"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBeveledSquareArtwork.cs", "59F15E7043E7885A6B6DAF8959434538D412E950032E729C92B585CA78D7A114"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuThinBorderArtwork.cs", "33900695200F98005D9DA22CF40A819C73F996D84A706D60531B73501C81E499"),
             new("csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs", "D9962D54AFBEFB46100A86DE2AD659010A1AC7E6F259CFC69DBD63A115F3F462"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapSpriteDefinitions.cs", "DAB81FFF6B9F3D3E119C11DAA5325EF9456AF899FDE3DA558AE7942CCEF1E4B8")]),
        new("SuperMetroid.Core.Assets.MapArrowPresentation", "map-arrow-complete-direction-domain", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapArrowPresentation.cs", "87EEE91D1662D71E12DEAD7C6E2619023E5DD390F39FE9E9B5ABE08789E8338F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "B4CACFDB54AC66C639716216773559EB030B1AF1AAD935EEBC4A78404C17E71F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapArrowDefinitions.cs", "6777899DC0AE8CE8F136390859A7067328E6EA840165872E4672C09944F1338D"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs", "B2A088287D71E35EB00E96AF63BD4D529B8E7697BC835D1E1BD23024808AE212"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelectMapScroll.cs", "1CCF645F03EF3DC6CD35FFF366952A2EA4057E32C9E34C23199EE2BAAC20C292")]),
        new("SuperMetroid.Core.Assets.MapScreenPresentation", "map-screen-complete-pages-and-bounded-names", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs", "6F448A58F6988DBB7B0AF9EAC6C07622181301F6B2E99810F7421B0CA287A3AA"),
             new("csharp/src/SuperMetroid.Core/Assets/MapScreenDefinitions.cs", "D30468DBD19FE12DDE457A0B27E0B6EA9C955E3CD297A712D9316ACFC2CFB61B"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "34967995C1C41594033A1B1824A26C2181EE2D90862E68A15EF36D53899F35B5")]),
    ];
}
