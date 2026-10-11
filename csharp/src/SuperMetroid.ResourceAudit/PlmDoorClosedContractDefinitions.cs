namespace SuperMetroid.ResourceAudit;

/// <summary>Source-reviewed flat door and single-word PLM artwork; aliases are owned by compiled definitions.</summary>
internal static class PlmDoorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "1DA6DFC4917DCCCBD291864836978A1545C55A3D3C03A81B20D943885938BEAC");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog", "plm-blue-door-complete-aliased-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBlueDoorVisualCatalog.cs", "56C4333FCE13C16A1D95569E22D7626F5FE4C379013B9309D2D1686CDC7C3BB3"),
             new("csharp/src/SuperMetroid.Core/Rooms/BlueDoorPlmDrawDefinitions.cs", "AB51DDA843F21BC41BF50548CC6A6689C12C890992F840AE2E17BEFFE1BDC3E3")]),
        new("SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmColoredDoorVisualCatalog.cs", "672440634FB056D441257A3862F4CE0D95E7ADB6E4D2F76B4E4BD5C5B60948C8"),
             new("csharp/src/SuperMetroid.Core/Rooms/ColoredDoorPlmDrawDefinitions.cs", "76C0A14E3637A2A5E3DE37D2464CEDF2D697BE84938E372E1D1E81DDF317CE46")]),
        new("SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGreyDoorVisualCatalog.cs", "4D525A98CA8710FC223E6D09EB651CEE224ABD5E0C837CE30BF7F0DCACDAEFC9"),
             new("csharp/src/SuperMetroid.Core/Rooms/GreyDoorPlmDrawDefinitions.cs", "CFF094ADA89BAA9B65F3F8EF9DCA20EC85FDB616A9C8CB4D589F423EE95375A1")]),
        new("SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEyeDoorVisualCatalog.cs", "47C128195669FE1AF632049145FFEDBB50F474816E3278A23918531FA3DED0C9"),
             new("csharp/src/SuperMetroid.Core/Rooms/EyeDoorPlmDrawDefinitions.cs", "CBF7F022257BF462890E36DC193697C3BD4B0AE930255918BBB9397E3ADDBAC8")]),
        new("SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEscapeGateVisualCatalog.cs", "2C41BDBFC81485E25055B22DE45B2E41CCB41A3D53D691C28942397E2C399C3D"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainEscapeGatePlmDrawDefinitions.cs", "C81C3183BB1200BED668179AFB9AD278FE8CC0908D5B0293415EFE81A10A5F2D")]),
        new("SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleVisualCatalog.cs", "2E34BE68E5702BF5C13C5E4999E4979AD53995BECA0903C386EF677C8F53208D"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleDrawDefinitions.cs", "A7459A94768B409116B7902B01252B84FED22E996FED7081A847FF4E5A150996")]),
        new("SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockVisualCatalog.cs", "985EDADC1E29EB8DD7FD8BA23D9278B9312C33D7A48F486D69558142EA550755"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockDrawDefinitions.cs", "3195804BF8A11E717671DFE230E9E553A9576CFE1A4183B369B75CDBE78EE5F8")]),
    ];
}
