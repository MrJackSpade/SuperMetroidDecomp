namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "1DA6DFC4917DCCCBD291864836978A1545C55A3D3C03A81B20D943885938BEAC");
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs", "403B861950E7121C78491A40F6232910987F55BCB55F1253B84C63016A86AB4C");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmTourianAccessVisualCatalog.cs", "5E7B73B49ADFC3D356D24F18FC33B0264720CA362C3BCD6AB8BEFF872AFD4B94"),
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "1281FE1633766941CD586A13F6CCDE3B50EBE575026EE6547C0FB18DA4711F75")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "ED6B308007D200997CA60EC4873A12CF2D53416EE6DF54729DC94078D4E0EA58"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "75F04CD495A7511038602B09675F529348D626A744CB6905BA38E37050BE56AD"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "C724BCB80C826229EB818EE9827AF6253312402EC505E5338BA25FF925BE4E01")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "E44B8D6B1591FFCE3A7FCB9E1535A4A1BB59063B8894F04256D0B8222EBF295A")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "85312B4F68D04BC84ED0FDB3805AC6F9F2AA44E79DE1B099D278671C8CAA4460"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "8B322BD74E14259F8A6EFE7F54E9F55D80DA49714BEFA40A3758806CEA14A3B4")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "039CE83E19C7411BF7A157D40D1AC0E1DE8B0AE8BB89DCA62EA03297BA5E7DD5"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "3AF891FBF971AAC9B7FE3809E330492F7F0C5D010CC228F7E9D543C8ED5DF2A6")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBotwoonWallVisualCatalog.cs", "C64CCC3DC7A673F359377405A35255A54571BE8B7D536628FE1EE65BCAFCF4C3"),
             new("csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs", "1D586E3A53B3791E4BFD7ABC27754F0E0862DD7FA9925A71A65A140E8D35BCB0")]),
        new("SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmKraidVisualCatalog.cs", "73D40A72EDBCFA63CC66CCD21A59544E20BF5540E1843F93C8C5ABCFCFD82524"),
             new("csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs", "A2A14ED4FE73A072B64A73840B1EB5F3FBADBE58F80ED62030F086B44ECDC497")]),
        new("SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "D188D3BA42671E97D48D51CFFD2CB4B8C44739EEA7D4F3C9C2EA690E1A5222D2"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "888BED009E3FB054A98D1F822CF54928C1DC56CC61F1D8A11DFA19C516E41113")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "DB42318205C9759EE17ED771CCE62752ED4ECF810A9D785B8A5BD07616478CB3"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "3E220A5224E772FE0D1112BDB0871D0DB906F67EA532C8415F0975ABBE8E73A3")]),
    ];
}
