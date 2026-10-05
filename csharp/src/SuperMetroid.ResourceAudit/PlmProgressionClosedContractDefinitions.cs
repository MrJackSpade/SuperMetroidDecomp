namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs",
        "BC718B3E5F2C342613E011FCFCE182CABBC54674A41F7F38A70BEBABDEC49A48");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmTourianAccessVisualCatalog.cs", "3F0707A0985A007399871B76F2F2457FD5FFCF379D4B8C88270D41B7012E79A5"),
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "D028AAE6CF0E45642BD9AFB3F0FDF3C78EEBF54DAC77C3DCD589826F518B5616")],
            "The constructor validates all five unique known crumble/clear layouts and clones only frames differing from calculated stock. GetWord guards their native run/word shapes, including all six clear rows. Floor mutations and event timing are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "CC1CED1AA9404BD570E700E43D0426C8E17F3EC2D4482459E1487981E25DB8A7"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "F78EF49D8B550E9D03254A76852BBF842416D4FE7E2284435BAAB2AA67D635B8"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "4ABFB82191103F503067AC5469BB5DF8A36CAA68A8E594F881CCE61FB04DC32C")],
            "The constructor accepts exactly one known one-word reveal and stores its value independently of the input array. GetWord accepts only that pointer, run zero and word zero. The source owning the reveal pointer is guarded; speed-block mechanics are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "33605EC2FA3DE6BEA9C70C7F37FF198584FA45CD161F0A1EFA357AA81A43E9A4")],
            "The constructor accepts exactly one known one-word draw and stores its value independently of the input array. GetWord guards the sole pointer/run/word tuple. Door setup, sound, holds and deletion are outside this resource proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "53A52B79EAF29266EAD359268AEBDD35498212DB64AB7450FDD0D41CA6641AC4"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "5CC19E2F78F4FE452967897B8F73159DF48B655C4B8A0DAC09BAC7FD61936B27")],
            "The constructor validates all four unique known ceiling frames and clones only frames differing from calculated stock. GetWord guards each two-word run separately before flattening. Ceiling collision and crumble timing are not executed or modified."),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "733D084A2EAAC53579A93B0C00E5B31D93ACF6EE01F994F66E2F5450D8741BC5"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "4013297F2748429D0AD81C9CC0234F21CC70BB859B195E6EBAA02203825B9BDA")],
            "The constructor validates all eight unique floor/ceiling pose identities and clones only frames differing from calculated stock. GetWord guards the two/two/four native run widths before flattening; a narrow run cannot borrow the wide run's capacity. Grab behavior is outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBotwoonWallVisualCatalog.cs", "D2AC19990BDC21336FEFA4B06D8101846639F948C23B80E671937C8FF027C73F"),
             new("csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs", "B68729863B43AE13C8E40A2C92724585A262BD0F2FF8E7254A388BAD4A6E6C0C")],
            "The constructor requires exactly one known nine-word clear frame; stock uses the calculated fill and custom payloads are cloned. GetWord guards the sole pointer/run and nine-block bounds. Crumble frames use the separately reviewed shot-block provider; no boss event is inferred."),
        new("SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmKraidVisualCatalog.cs", "5D8B222F34350E44F45744445E1FDF7F2B78F7F61A85C411C431F489981B14E9"),
             new("csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs", "1A450BDDA30C125E40BAA9F73664413CF3B3F8517795719828CF06589FEB5310")],
            "The constructor requires all ten unique known ceiling/spike layouts and clones exact payloads. GetWord guards their individual widths, including fifteen-word ceiling and twenty-two-word spike clears. The shared elevatube pointer source is guarded. Death/event sequencing is not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "AD49C64521A11AAADBDC136DBF9997E77E53AB86CD2AEA123BA92E62BF38E158"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "13B49410B20354EE61FF20599BB9D07E880233505BF8D6B304F3458283D9BCDB")],
            "The constructor requires all five unique known bridge/wall draws and clones only custom frames. Stock words are calculated from native geometry. GetWord guards each run and width before custom or stock selection; flattened content identity is preserved. Arena mechanics and boss phases are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "6A6110A2B9BCDF652B8E9BE5D14995A01BF231B2C3D5F3D7177491D4CE3C010C"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "33BF8E9A8DF00C7D7C87F2E0646B92AAF32B18D50D24AD2C58C213729BC487E2")],
            "The constructor requires all twenty-two unique compiled background/door/tube layouts, including the two unused-but-owned rows, and clones only customized frames. Stock visual bits project directly from physical cells without a duplicate cache. GetWord guards asymmetric run shapes before flattening and preserves the existing content identity. Ownership does not claim reachability or certify battle timing."),
    ];
}
