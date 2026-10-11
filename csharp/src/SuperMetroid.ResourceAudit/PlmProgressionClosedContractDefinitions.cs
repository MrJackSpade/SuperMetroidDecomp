namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "AD425ED952C838A1FB7836858E04FF2AD66D5E475562D1220F69A9F45B29F335");
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs", "CAA7DF90840BE387A6713EDE8EC84621E1DF704ADF2FFC4C651CB4CAB502A1C0");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmTourianAccessVisualCatalog.cs", "5E7B73B49ADFC3D356D24F18FC33B0264720CA362C3BCD6AB8BEFF872AFD4B94"),
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "7E72A860813AF193C632DEC23EE0B6596D491D082C50264B1B8D002562B248F4")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "ED6B308007D200997CA60EC4873A12CF2D53416EE6DF54729DC94078D4E0EA58"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "37FFCDA72B530AB4CFB2B873C9A81152F863CE1F77C2A8E144D0C59E9DCB95E4"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "C19227224F25F1EC1B8A4544CD20FA8D53F7C3A5C5C989CC73AD716857B67987")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "E44B8D6B1591FFCE3A7FCB9E1535A4A1BB59063B8894F04256D0B8222EBF295A")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "85312B4F68D04BC84ED0FDB3805AC6F9F2AA44E79DE1B099D278671C8CAA4460"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "22692DE30D585CAE8A659BEA3160D721810211C753A6EF0F89B004B0F7F48EE3")]),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "039CE83E19C7411BF7A157D40D1AC0E1DE8B0AE8BB89DCA62EA03297BA5E7DD5"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "00E48DFAA36AAFB1D0779EBA1F5444216F9AA00C11940D49F2A9D797AA32085C")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBotwoonWallVisualCatalog.cs", "C64CCC3DC7A673F359377405A35255A54571BE8B7D536628FE1EE65BCAFCF4C3"),
             new("csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs", "8E57692311EA82C67D58C1B211E991FFD159F702C18B62DBE35F7A80808AE858")]),
        new("SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmKraidVisualCatalog.cs", "73D40A72EDBCFA63CC66CCD21A59544E20BF5540E1843F93C8C5ABCFCFD82524"),
             new("csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs", "4130EE9E9886BD7AF8047B99D4769140E190A37114547A7A7FE480258AE7B7B4")]),
        new("SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "D188D3BA42671E97D48D51CFFD2CB4B8C44739EEA7D4F3C9C2EA690E1A5222D2"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "02E48751A9D204721E3355FDFF7CDDED36AD5EB407B002AEBAA40F4F872469B9")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "DB42318205C9759EE17ED771CCE62752ED4ECF810A9D785B8A5BD07616478CB3"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "9E7C8D92462B8D71CDCA013C4B7D2983A37C979D6C614844A90BA0714344DC31")]),
    ];
}
