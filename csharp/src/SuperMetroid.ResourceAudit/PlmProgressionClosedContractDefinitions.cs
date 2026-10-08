namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs",
        "1B9795F3849300D62D7D7C2E7689DAC1E702754CDB0B1C439C859B289C787DB3");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmTourianAccessVisualCatalog.cs", "64978941DFB8D36E35728C38E59FFB465100269615AA63848FDF371E5C306535"),
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "D028AAE6CF0E45642BD9AFB3F0FDF3C78EEBF54DAC77C3DCD589826F518B5616")],
            "The constructor validates all five unique known crumble/clear layouts and clones only frames differing from calculated stock. GetWord guards their native run/word shapes, including all six clear rows. Floor mutations and event timing are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "19CC90AFC412B2E974227C388C8F3E05D22A83559152F5C3AD7B6C17E1B45589"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "3D1D8EDF0CBA5946331129B701AEA4B780CE874D405B091F4E63C4C0C62725EF"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "63698BB75FD940A39A253930C8E206A18AB4E8392B767173AA048832C7C14F5B")],
            "The constructor accepts exactly one known one-word reveal and stores its value independently of the input array. GetWord accepts only that pointer, run zero and word zero. The source owning the reveal pointer is guarded; speed-block mechanics are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "33FF2B04E1DEE8F8B3871BE65B6EE3CD5DCF516B6DCABEFB1F6F7C0CC8B7FA98")],
            "The constructor accepts exactly one known one-word draw and stores its value independently of the input array. GetWord guards the sole pointer/run/word tuple. Door setup, sound, holds and deletion are outside this resource proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "A1FF9A28A5A28357576BCA9CB6BE6FC3C84885B6077FCE30FFCCEE72A68B9B86"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "5CC19E2F78F4FE452967897B8F73159DF48B655C4B8A0DAC09BAC7FD61936B27")],
            "The constructor validates all four unique known ceiling frames and clones only frames differing from calculated stock. GetWord guards each two-word run separately before flattening. Ceiling collision and crumble timing are not executed or modified."),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "98196B7BDCAD1DA5BD03B9AEECE0FDD28A2C9B6696BF8DD88F0C7D94E1A95349"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "4013297F2748429D0AD81C9CC0234F21CC70BB859B195E6EBAA02203825B9BDA")],
            "The constructor validates all eight unique floor/ceiling pose identities and clones only frames differing from calculated stock. GetWord guards the two/two/four native run widths before flattening; a narrow run cannot borrow the wide run's capacity. Grab behavior is outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBotwoonWallVisualCatalog.cs", "C469328CB1862D8BC6582A3A55456A20B36C3609300AA19DD736940CF2C6C1AD"),
             new("csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs", "B68729863B43AE13C8E40A2C92724585A262BD0F2FF8E7254A388BAD4A6E6C0C")],
            "The constructor requires exactly one known nine-word clear frame; stock uses the calculated fill and custom payloads are cloned. GetWord guards the sole pointer/run and nine-block bounds. Crumble frames use the separately reviewed shot-block provider; no boss event is inferred."),
        new("SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmKraidVisualCatalog.cs", "CA5BC3D390FBD40A5A0905E14534555A6F5B3248B0C08095A80BE5CC60E3984E"),
             new("csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs", "1A450BDDA30C125E40BAA9F73664413CF3B3F8517795719828CF06589FEB5310")],
            "The constructor requires all ten unique known ceiling/spike layouts and clones exact payloads. GetWord guards their individual widths, including fifteen-word ceiling and twenty-two-word spike clears. The shared elevatube pointer source is guarded. Death/event sequencing is not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "0E11FA9B3685132FE15E32D9AC28C74D83E1B1859C856BB618E8E9BECBFC7FB1"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "13B49410B20354EE61FF20599BB9D07E880233505BF8D6B304F3458283D9BCDB")],
            "The constructor requires all five unique known bridge/wall draws and clones only custom frames. Stock words are calculated from native geometry. GetWord guards each run and width before custom or stock selection; flattened content identity is preserved. Arena mechanics and boss phases are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "B4039BE2661E865E7E6628BDBE2830DF207754866CD1FE2AE14AA490567D4490"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "B1F9F4958F2E812E1A265EAD74ACF191794C2174B2809A4CD21E56B34665A4E4")],
            "The constructor requires all twenty-two unique compiled background/door/tube layouts, including the two unused-but-owned rows, and clones only customized frames. Stock visual bits project directly from physical cells without a duplicate cache. GetWord guards asymmetric run shapes before flattening and preserves the existing content identity. Ownership does not claim reachability or certify battle timing."),
    ];
}
