namespace SuperMetroid.ResourceAudit;

/// <summary>Source-reviewed flat door and single-word PLM artwork; aliases are owned by compiled definitions.</summary>
internal static class PlmDoorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "A12FA434EA63721B9FA6209C32B200522701C72552730B35052249373814E22C");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog", "plm-blue-door-complete-aliased-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBlueDoorVisualCatalog.cs", "56C4333FCE13C16A1D95569E22D7626F5FE4C379013B9309D2D1686CDC7C3BB3"),
             new("csharp/src/SuperMetroid.Core/Rooms/BlueDoorPlmDrawDefinitions.cs", "CE5913066D47541C4D9C24CD0F16496AAE9FA80FA848618EF415FCF4324C5703")]),
        new("SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmColoredDoorVisualCatalog.cs", "672440634FB056D441257A3862F4CE0D95E7ADB6E4D2F76B4E4BD5C5B60948C8"),
             new("csharp/src/SuperMetroid.Core/Rooms/ColoredDoorPlmDrawDefinitions.cs", "3324735B2020F89ABD04C0A9BEED15CF91D059161C0EA3BD6392037E1FA1E2AA")]),
        new("SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGreyDoorVisualCatalog.cs", "4D525A98CA8710FC223E6D09EB651CEE224ABD5E0C837CE30BF7F0DCACDAEFC9"),
             new("csharp/src/SuperMetroid.Core/Rooms/GreyDoorPlmDrawDefinitions.cs", "AC979B283C371CF20D7C514EC993E44D0E06661DB05F2662321D60888E3AF57E")]),
        new("SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEyeDoorVisualCatalog.cs", "47C128195669FE1AF632049145FFEDBB50F474816E3278A23918531FA3DED0C9"),
             new("csharp/src/SuperMetroid.Core/Rooms/EyeDoorPlmDrawDefinitions.cs", "3D3A1448AA49DEA7F7AD11B02DF3976EC546BB59CBC76CC37CB184EF8660DD1A")]),
        new("SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEscapeGateVisualCatalog.cs", "2C41BDBFC81485E25055B22DE45B2E41CCB41A3D53D691C28942397E2C399C3D"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainEscapeGatePlmDrawDefinitions.cs", "630CD38AE30D23505DFCA23B498EA3BEB4E0B0ECC41092BDE4F965290E29D13E")]),
        new("SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleVisualCatalog.cs", "2E34BE68E5702BF5C13C5E4999E4979AD53995BECA0903C386EF677C8F53208D"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleDrawDefinitions.cs", "3D233386EEA7F87AA2335CD68D581BAD4F273F35DB41F4A2869D6EEBBCDB8038")]),
        new("SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockVisualCatalog.cs", "985EDADC1E29EB8DD7FD8BA23D9278B9312C33D7A48F486D69558142EA550755"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockDrawDefinitions.cs", "3195804BF8A11E717671DFE230E9E553A9576CFE1A4183B369B75CDBE78EE5F8")]),
    ];
}
