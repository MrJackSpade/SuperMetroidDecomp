namespace SuperMetroid.ResourceAudit;

/// <summary>Source-reviewed flat door and single-word PLM artwork; aliases are owned by compiled definitions.</summary>
internal static class PlmDoorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog", "plm-blue-door-complete-aliased-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBlueDoorVisualCatalog.cs", "4B051251DC9A26C6F4308643181AE8AF53383AB4AABCA369D7A14EDD09D549C5"),
             new("csharp/src/SuperMetroid.Core/Rooms/BlueDoorPlmDrawDefinitions.cs", "7A85AB5A317935DDDEF24AF13933C2228FCB200DED4B9D447DD466CCA416332C")],
            "The constructor requires sixteen unique known four-word authored frames and clones every payload. Four closed-cap pointers resolve to required opening frames through the guarded VisualSource mapping; all twenty supported pointers have four words. Unknown pointers are not accepted by aliasing. Collision and opening timing are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmColoredDoorVisualCatalog.cs", "57852600BDA37D9B0CADA4FE32228346A7A05AFF0ADA1636BF221831454FFE15"),
             new("csharp/src/SuperMetroid.Core/Rooms/ColoredDoorPlmDrawDefinitions.cs", "B41D775E5EC8A09005D1E9BE42ED10C05DEC70AA609DDA8B878E472E2EBEFA9E")],
            "The constructor requires all forty-eight unique known yellow/green/red orientation frames with exact cloned four-word payloads. GetWord guards the pointer and array bounds. Projectile filters, persistence and conversion to blue caps are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGreyDoorVisualCatalog.cs", "9533DFF0CE05C213EA9E2E774836C0B1CE83A96D22CAA9805156039743251B5A"),
             new("csharp/src/SuperMetroid.Core/Rooms/GreyDoorPlmDrawDefinitions.cs", "C105E863132F7FF51358E917B1D64331FB684B7F1108A1F56B7B7C1159E92179")],
            "The constructor requires all twenty unique known grey/clear frames with exact cloned four-word payloads. GetWord guards the pointer and array bounds. Condition gates, timers and sounds are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEyeDoorVisualCatalog.cs", "263ABAB3229A5156D3EBC6E3504AAAA093446FD26F80BC424134B49F5D59F073"),
             new("csharp/src/SuperMetroid.Core/Rooms/EyeDoorPlmDrawDefinitions.cs", "3C8EDDF7CCD760EDCBF239CDC96714F62D71EA5B68B708CD725220BDDF97063C")],
            "The constructor requires twenty-three unique authored frames with exact cloned one/two/four-word payloads. The twenty-fourth pointer uses the required four-word clear frame with horizontal flip. GetWord guards the mapped frame's array bounds; known narrow components cannot borrow clear-frame width. Eye attacks and conversion timing are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEscapeGateVisualCatalog.cs", "D585F90656F9493C4FB54FD1EF20A09A75C56285941DBA5D090E7DA8E1C57511"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainEscapeGatePlmDrawDefinitions.cs", "A020A85763B93F69C7292FEF49EE3E086382DAD5F894CD7D4522434F91EF7036")],
            "The constructor requires all three unique known escape-gate frames with exact cloned four-word payloads. GetWord guards the pointer and array bounds. Escape progression, physical words and animation cadence are not altered."),
        new("SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleVisualCatalog.cs", "FEE1DB81BA44A97AE7CCA6E600F1626854218D2B9A45A7BAF5743C08F1D34C27"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleDrawDefinitions.cs", "8FE68FA48E75939845847890599136FC7CFC36B899700DDCEFBF69F72D5F21B0")],
            "The constructor requires all twenty-four unique known collectible frame identities and stores independent visual words. GetWord guards membership in that complete pointer set. Pickup effects, graphics allocation and item persistence are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockVisualCatalog.cs", "928B33A7F718A82B5523D2EACADDB4417222B5F05D2D94FEB816370F40BB6499"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockDrawDefinitions.cs", "1FBE71AE2509E5F452C106E9EE7B7ADA322A61B8AE87C6E4FC32C4CE5CFD0858")],
            "The constructor requires all five unique known Grapple-block frame pointers and stores independent visual words. GetWord guards that complete set. Grapple collision, breaking and respawn mechanics remain outside this resource proof."),
    ];
}
