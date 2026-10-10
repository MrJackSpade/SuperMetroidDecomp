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
            [new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "595FA48EE3F54462D44422E1739F0782866FB3673B899E9218B00FEBEE270BB5"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerGeometry.cs", "D6F3921874E7FE5D1E2A6A96D1A9CD2E5C9DF06E3517419E18DE016B2F26ED0A"),
             new("csharp/src/SuperMetroid.Core/Assets/WorldMapLabelComposition.cs", "E96C640C2B0F7B9A0AEBA0AACAF27683CA2DE1646628FD65FE7E7C274E1FC439"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuSmallFontArtwork.cs", "E7366DAF5100AC184A142A8DB25121215ED1804480C6BC963BC315145FAB1CCA"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuLargeFontArtwork.cs", "A4D7EE7AD770EA6F73106177EE920009D4B4CF8D58E7868CBF2602EE688BC156"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCompactLetteringArtwork.cs", "6A04CABAAB759493AE80FC3E9B2F36D93DB2AED16B42768B5437F0CFA9C60C42"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuPanelTileArtwork.cs", "DC80646A4C4BF8C0570EB76B3BA9C6289AA232B0418A51282A54D96335E5602B"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderButtonArtwork.cs", "9F6642E25A904E42CDA64731CA8E1E4D2955E715E80C64E4505A427495F9C7B5"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderHighlightArtwork.cs", "2D4CEA7FBE03EE0703E501F75203F0D1A4A2C0C0401410AD378FEC3A01181DC9"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerTileArtwork.cs", "3646900A2E2929DB054D595C262BFD6F564B7E803899EB63F4978D71CFD1606C"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBeveledSquareArtwork.cs", "53C9DD9211304FA72AA4827A7D01130CB402963519011FB42FF363B6008266F5"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuThinBorderArtwork.cs", "E90294A3169DC238AFE9EE449053399E3E911F064981F2FB975C95AF475D6EB5"),
             new("csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs", "4D85E1F9155FF5F43A500295CEB2B1958261AAFD051CC2FF8814EB7C77E048D8"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapSpriteDefinitions.cs", "DAB81FFF6B9F3D3E119C11DAA5325EF9456AF899FDE3DA558AE7942CCEF1E4B8")]),
        new("SuperMetroid.Core.Assets.MapArrowPresentation", "map-arrow-complete-direction-domain", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapArrowPresentation.cs", "C7B9F8713534F9A00B1D1C84B78DF2BBA7AC23F40EDC1CD7192C9135D1D21950"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "B4CACFDB54AC66C639716216773559EB030B1AF1AAD935EEBC4A78404C17E71F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapArrowDefinitions.cs", "6777899DC0AE8CE8F136390859A7067328E6EA840165872E4672C09944F1338D"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs", "0A3FBE3E6F98C5894F078236938C141666481F786A6D4FECA807E9C6787B47F6"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelectMapScroll.cs", "572987D51CE0C231D94B6A8B37AF2D28573F3A0B92DF86AF9524436490CE26A1")]),
        new("SuperMetroid.Core.Assets.MapScreenPresentation", "map-screen-complete-pages-and-bounded-names", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs", "6F448A58F6988DBB7B0AF9EAC6C07622181301F6B2E99810F7421B0CA287A3AA"),
             new("csharp/src/SuperMetroid.Core/Assets/MapScreenDefinitions.cs", "D30468DBD19FE12DDE457A0B27E0B6EA9C955E3CD297A712D9316ACFC2CFB61B"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "34967995C1C41594033A1B1824A26C2181EE2D90862E68A15EF36D53899F35B5")]),
    ];
}
