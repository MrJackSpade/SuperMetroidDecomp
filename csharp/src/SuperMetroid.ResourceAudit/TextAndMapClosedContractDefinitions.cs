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
            [new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "4E1AAB0B1D9CB2BD66BA3FEE694453CBCD9FFF4ADA798FAAED37D14C91B8598B"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerGeometry.cs", "D6F3921874E7FE5D1E2A6A96D1A9CD2E5C9DF06E3517419E18DE016B2F26ED0A"),
             new("csharp/src/SuperMetroid.Core/Assets/WorldMapLabelComposition.cs", "0054E8A3EB1B7CC5EEBF535D677648C9B01A5677ACF08BD0B0B804537614FDC2"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuSmallFontArtwork.cs", "E7366DAF5100AC184A142A8DB25121215ED1804480C6BC963BC315145FAB1CCA"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuLargeFontArtwork.cs", "A4D7EE7AD770EA6F73106177EE920009D4B4CF8D58E7868CBF2602EE688BC156"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCompactLetteringArtwork.cs", "6A04CABAAB759493AE80FC3E9B2F36D93DB2AED16B42768B5437F0CFA9C60C42"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuPanelTileArtwork.cs", "499E8FB8F43DD78841C4B75D224500C8B31BABEF911DD0C6B5C8450E1CC945C1"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderButtonArtwork.cs", "13948C912F3EF487E3BE84201C2802B5852752A4F679425C34449C23BEEE7FFC"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderHighlightArtwork.cs", "E9E37B389C03839C53EABC2A786A0F0D505FD55736D236F38776751A126346F3"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerTileArtwork.cs", "73FAB0B28464D6618AAAF9813F9416D33021695A5E00FE4B7C419229156EC7D3"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBeveledSquareArtwork.cs", "3AE42C165BBC17CFDA8D8FA0507277FBCD23FA99F6137282538FC5CC846EB260"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuThinBorderArtwork.cs", "8E822B275C2E56F6D25DD94FDD2DF471B830E526ED549E6FF35D49A0954EC563"),
             new("csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs", "9AC25AD8A0FB74A2D64FB0E7722BDF0393CE0534F94036C20688752C484052E6"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapSpriteDefinitions.cs", "DAB81FFF6B9F3D3E119C11DAA5325EF9456AF899FDE3DA558AE7942CCEF1E4B8")]),
        new("SuperMetroid.Core.Assets.MapArrowPresentation", "map-arrow-complete-direction-domain", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapArrowPresentation.cs", "B9E3454C558211B6F8C508549A784280355EFD44481AEDC37166126609F21206"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "B4CACFDB54AC66C639716216773559EB030B1AF1AAD935EEBC4A78404C17E71F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapArrowDefinitions.cs", "6777899DC0AE8CE8F136390859A7067328E6EA840165872E4672C09944F1338D"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs", "B2A088287D71E35EB00E96AF63BD4D529B8E7697BC835D1E1BD23024808AE212"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelectMapScroll.cs", "AC4957D1199E811D948F341A7B427FD872EC7BCE379232B065FB9751CCCD6865")]),
        new("SuperMetroid.Core.Assets.MapScreenPresentation", "map-screen-complete-pages-and-bounded-names", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs", "6F448A58F6988DBB7B0AF9EAC6C07622181301F6B2E99810F7421B0CA287A3AA"),
             new("csharp/src/SuperMetroid.Core/Assets/MapScreenDefinitions.cs", "D30468DBD19FE12DDE457A0B27E0B6EA9C955E3CD297A712D9316ACFC2CFB61B"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "34967995C1C41594033A1B1824A26C2181EE2D90862E68A15EF36D53899F35B5")]),
    ];
}
