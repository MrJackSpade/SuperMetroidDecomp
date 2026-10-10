namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "9985A5C60C510351018F1DDBE431A8E39C599D61233AB8F678B7946BC1290782");
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs", "D036DCF1F294E96033A51FD7E85041855BB00ABCD1E1B5D11A2F62ED4B513C66");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmTourianAccessVisualCatalog.cs", "5E7B73B49ADFC3D356D24F18FC33B0264720CA362C3BCD6AB8BEFF872AFD4B94"),
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "76AF3C76CC57D6208C7BD4C2FDFB724933F2CF8FADC78E4344C0703183EF22C2")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "ED6B308007D200997CA60EC4873A12CF2D53416EE6DF54729DC94078D4E0EA58"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "75F04CD495A7511038602B09675F529348D626A744CB6905BA38E37050BE56AD"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "96302C27E6286E087C15C867848E85C86F2E85CDAE9D1EF79F2C14691DD3FB08")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "E44B8D6B1591FFCE3A7FCB9E1535A4A1BB59063B8894F04256D0B8222EBF295A")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "85312B4F68D04BC84ED0FDB3805AC6F9F2AA44E79DE1B099D278671C8CAA4460"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "8BD887000A3421C91233FAC3B50ACC60F31B13C0BE7DDF098D16251C0AB24FFE")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "039CE83E19C7411BF7A157D40D1AC0E1DE8B0AE8BB89DCA62EA03297BA5E7DD5"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "ECE8D1600FED63C435BC0D875E191460A83B860AAC0DDB95B3D3F0ABC63EDC80")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBotwoonWallVisualCatalog.cs", "C64CCC3DC7A673F359377405A35255A54571BE8B7D536628FE1EE65BCAFCF4C3"),
             new("csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs", "1D586E3A53B3791E4BFD7ABC27754F0E0862DD7FA9925A71A65A140E8D35BCB0")]),
        new("SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmKraidVisualCatalog.cs", "8ACED9E97E493AEE1293114FDCB655AF77F8B153FB2ED007ADFE40CC4F2E1748"),
             new("csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs", "8A55092995A726521E087AFC122CBBFF694B59F30E7A176FAAA583C5A6BECB3C")]),
        new("SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "D188D3BA42671E97D48D51CFFD2CB4B8C44739EEA7D4F3C9C2EA690E1A5222D2"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "4101CCF5A2D77FA406C29C0A769D31F4383AB465D84E659B9BBF88A2D9D4D80D")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "DB42318205C9759EE17ED771CCE62752ED4ECF810A9D785B8A5BD07616478CB3"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "BCD58AA65E6114169BB59C8BB6D4298A89F11DD29DF52D30C55B11E29D5A2A9E")]),
    ];
}
