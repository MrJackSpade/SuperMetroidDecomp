namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed text and map loaders with complete finite resource domains, not cinematic or navigation tests.</summary>
internal static class TextAndMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EscapeTypewriterPresentation", "escape-text-complete-program-membership", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs", "AD5C59F78F8A1F494545B46609AB0FF4A31E2E087AAE515F09293C66918D9177"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterDefinitions.cs", "0F881988966CED5AF34B1F21DBD73FDBD7CBA043418F37A939A11E02496D3C05")],
            "The sole private-constructor loader requires both non-None escape programs before publication. Get selects that complete membership set. Returned Lines arrays are mutable, so this proves program identity coverage only, not post-publication line/glyph integrity or typewriter behavior."),
        new("SuperMetroid.Core.Assets.IntroNarrationPresentation", "narration-complete-compiled-pages", ["GetLines", "Compile"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs", "E7C01AC369BDD560135F469FF8E5DEA39E44D50B69D30681A659B739501B16BD"),
             new("csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs", "E4C3A28CF6D33EC61265864EF63979C7A1C062CA50CA235162A3F90C8B1CB21C")],
            "Private construction requires all six pages with nonempty, ordered, bounded lines and supported glyphs. The loader copies arrays of immutable line records; Compile uses the same guarded glyph compiler. Complete page/glyph coverage does not certify caret, timing or scene transitions."),
        new("SuperMetroid.Core.Assets.EndingTextPresentation", "ending-text-complete-panels-and-sequences", ["Compile", "BuildResultPanel", "BuildCopyrightPanel"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs", "0C836E9A69CCB7417E555BA9477F11883EE1A6F042BB6D8D39A7E800C13FAB89"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextDefinitions.cs", "32ADCA17B0B06C96C0F94BA912D93FB68D6CEF489FDDA8C9A4A2994280A09961")],
            "Private construction validates exact templates and compiles every mandatory label, both ending sequences and the copyright panel. Arrays are independently compiled; panel methods return copies. Unknown sequence enums fail. Ending timing, percentages and rendering are not certified."),
        new("SuperMetroid.Core.Assets.CreditsPresentation", "credits-loaded-complete-row-domain", ["GetRow"],
            [new("csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs", "C9647D00D5FB22FC18EF9671884F8E4A0F6EE0B2C6F7F5DB7E4D2DFFE9404697"),
             new("csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs", "1B28C307BBE3161D3E8B12FC0B9399F23E5DF991347C31EAEEACD3DDF11B8B7A"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextDefinitions.cs", "32ADCA17B0B06C96C0F94BA912D93FB68D6CEF489FDDA8C9A4A2994280A09961")],
            "The production loader compiles all ordered lines and requires exactly 520 independent 32-word rows. GetRow has CLR index checks. The audit additionally rejects this proof if Core references the alternate verification factory outside its provider source. This is a production row-domain proof, not credits cadence or caller-index safety."),
        new("SuperMetroid.Core.Assets.MapSpriteCatalog", "map-sprite-complete-named-compositions", ["Draw", "LoadArtworkTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "48EB477C5AD5475F87D08CC849AEEAD4DF1453A907E06B57F0B7D00D1A206F2A"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapSpriteDefinitions.cs", "C51218FADBA0FAF0E447BE8FD1128E4432A9A582784D3C7635EA89A3F67457F0")],
            "The sole private-constructor loader requires every one of the 26 named compositions and the exact indexed character sheet. The definition array is private and exposed only as a read-only span. Draw selects the sparse compiled IDs; LoadArtworkTo transfers fixed loaded bytes. Placement and map animation are not certified."),
        new("SuperMetroid.Core.Assets.MapArrowPresentation", "map-arrow-complete-direction-domain", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapArrowPresentation.cs", "8F943D437C68DB76289712C155F286A084BD39C7DF5140C2BA84B926B4F619E0"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapArrowDefinitions.cs", "90D4789FAF7EA8413623FC0C2789C3EA997D8277000F37D6D15F33D3A1766CB5"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelectMapScroll.cs", "FEA3643274C4F9BBC3CBEB494982E0E9886ADB19D1978EFAF7399082B47517BC")],
            "Private construction requires Left, Right, Up and Down with bounded anchors and nonempty positive phase durations, copied into private arrays. Get guards the four one-based directions. None is not artwork. Input, map scrolling and timing remain outside this proof."),
        new("SuperMetroid.Core.Assets.MapScreenPresentation", "map-screen-complete-pages-and-bounded-names", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs", "3B78B7DD59C12C379482E374C7ABCCBF1178BCC0904CDF79AAC2DBCBDF5E4D7D"),
             new("csharp/src/SuperMetroid.Core/Assets/MapScreenDefinitions.cs", "A9EBD75603C9B7578A71AA351D1CB3F1605689BC6AE58CB3BEFE62DE4FE64AA6"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "6B88F8EE1B5ED42848833924AD9B5844B632D174C964D73E78D8E3774922E834")],
            "Private construction requires all thirteen named 1024-cell pages and compiles independent tilemap bytes. Names must be compiler constants/finite conditional sets or the two reviewed area-name factories, which reject areas outside the six Zebes maps. Arbitrary strings remain unresolved; VRAM placement, centering and navigation are not certified."),
    ];
}
