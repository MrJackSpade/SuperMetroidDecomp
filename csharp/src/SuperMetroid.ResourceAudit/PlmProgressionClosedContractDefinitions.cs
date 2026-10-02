namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs",
        "A8BF7A13320D197ED18F792CD9843710ED73A8A4A4A0658618B8D3004F0D9704");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmTourianAccessVisualCatalog.cs", "4A20EF95D7A1AAC9501A1F64F960B484AB72FAE24F25A8DF83EF20B38F506C2D"),
             new("csharp/src/SuperMetroid.Core/Rooms/TourianAccessPlmDrawDefinitions.cs", "FA7A48715E9A47CAE4124849E26C196FFFC04E4FDFB36C94F414CF1E6353A0DC")],
            "The constructor requires all five unique known crumble/clear layouts with exact cloned flattened payloads. GetWord guards their native run/word shapes, including all six clear rows. Floor mutations and event timing are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSpeedBoosterVisualCatalog.cs", "CC1CED1AA9404BD570E700E43D0426C8E17F3EC2D4482459E1487981E25DB8A7"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs", "DBFBA36207348FB4C7CC4516C1537F0131752AF0964120B0BC8D7AF5016A10D7"),
             new("csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmProgramDefinitions.cs", "4ABFB82191103F503067AC5469BB5DF8A36CAA68A8E594F881CCE61FB04DC32C")],
            "The constructor accepts exactly one known one-word reveal and stores its value independently of the input array. GetWord accepts only that pointer, run zero and word zero. The source owning the reveal pointer is guarded; speed-block mechanics are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMaridiaElevatubeVisualCatalog.cs", "A591FFBC59FD3DA15024DE29C648307FE6CAE91053FA456BC1A753B2610E21AA")],
            "The constructor accepts exactly one known one-word draw and stores its value independently of the input array. GetWord guards the sole pointer/run/word tuple. Door setup, sound, holds and deletion are outside this resource proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSporeSpawnCeilingVisualCatalog.cs", "D3D872F86C1FCF3342D72A2F11FA8F50DEAC9CE16B64F6886844D8F2E5CB3441"),
             new("csharp/src/SuperMetroid.Core/Rooms/SporeSpawnCeilingPlmDrawDefinitions.cs", "E0AAB8316BD72BE7E861B8BA77FC8BE071F5177BFE5AE056692B0E3D0F656174")],
            "The constructor requires all four unique known ceiling frames with exact cloned payloads. GetWord guards each two-word run separately before flattening. Ceiling collision and crumble timing are not executed or modified."),
        new("SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSamusEaterVisualCatalog.cs", "4BEA9C9689C7D5EED1501CAE059781F605D193E6887D6E4D0D7985CA058DE99A"),
             new("csharp/src/SuperMetroid.Core/Rooms/SamusEaterPlmDrawDefinitions.cs", "E12B7097C6AD793369855EE6EF543A9B9684925883BDF1D9E81510F9C7FCED85")],
            "The constructor requires all eight unique floor/ceiling pose identities with exact cloned payloads. GetWord guards the two/two/four native run widths before flattening; a narrow run cannot borrow the wide run's capacity. Grab behavior is outside this proof."),
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
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCrocomireVisualCatalog.cs", "A8C02FB2476D9FECC9275577BD661CDDE05FF15A69A017232C39DA84443270D9"),
             new("csharp/src/SuperMetroid.Core/Rooms/CrocomireArenaPlmDrawDefinitions.cs", "1FFBC18B91C3AD46004A2A92B6A7AEF6648E8E29DFCBCBAA4675543725B6D6A6")],
            "The constructor requires all five unique known bridge/wall draws and clones exact payloads. GetWord guards each native run before flattening, not an unrelated draw's maximum width. Arena mechanics and boss phases are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainFakeDeathVisualCatalog.cs", "5992F2F5927E71545098B708893F4FCBB148848A074414B4799E8B301A529C57"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainFakeDeathPlmDrawDefinitions.cs", "6C2F24B4EC799FB773A560EF5A3C7F69AD68CA40874C272097830D75CBBAB798")],
            "The constructor requires all twenty-two unique compiled background/door/tube layouts, including the two unused-but-owned rows, and clones exact payloads. GetWord guards asymmetric run shapes before flattening. Ownership does not claim reachability or certify battle timing."),
    ];
}
