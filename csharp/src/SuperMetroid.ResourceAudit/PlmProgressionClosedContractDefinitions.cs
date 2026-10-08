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
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "D028AAE6CF0E45642BD9AFB3F0FDF3C78EEBF54DAC77C3DCD589826F518B5616")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "19CC90AFC412B2E974227C388C8F3E05D22A83559152F5C3AD7B6C17E1B45589"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "3D1D8EDF0CBA5946331129B701AEA4B780CE874D405B091F4E63C4C0C62725EF"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "63698BB75FD940A39A253930C8E206A18AB4E8392B767173AA048832C7C14F5B")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "33FF2B04E1DEE8F8B3871BE65B6EE3CD5DCF516B6DCABEFB1F6F7C0CC8B7FA98")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "A1FF9A28A5A28357576BCA9CB6BE6FC3C84885B6077FCE30FFCCEE72A68B9B86"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "5CC19E2F78F4FE452967897B8F73159DF48B655C4B8A0DAC09BAC7FD61936B27")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "98196B7BDCAD1DA5BD03B9AEECE0FDD28A2C9B6696BF8DD88F0C7D94E1A95349"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "4013297F2748429D0AD81C9CC0234F21CC70BB859B195E6EBAA02203825B9BDA")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBotwoonWallVisualCatalog.cs", "C469328CB1862D8BC6582A3A55456A20B36C3609300AA19DD736940CF2C6C1AD"),
             new("csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs", "B09D9072F3F9BDAA1D1B9D3D6C5EDF543550645778D208A9D5D8CD82D55C375C")]),
        new("SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmKraidVisualCatalog.cs", "CA5BC3D390FBD40A5A0905E14534555A6F5B3248B0C08095A80BE5CC60E3984E"),
             new("csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs", "41CC27F3F7E0233B6192469FE187D8C4DA1B53C15E9CD438FB348CE5DE1B5E74")]),
        new("SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "0E11FA9B3685132FE15E32D9AC28C74D83E1B1859C856BB618E8E9BECBFC7FB1"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "11F27AD767900E6AD184512E22E14CFF73ABE33D4BB7B68097F9D91D5EC65A46")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "B4039BE2661E865E7E6628BDBE2830DF207754866CD1FE2AE14AA490567D4490"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "B1F9F4958F2E812E1A265EAD74ACF191794C2174B2809A4CD21E56B34665A4E4")]),
    ];
}
