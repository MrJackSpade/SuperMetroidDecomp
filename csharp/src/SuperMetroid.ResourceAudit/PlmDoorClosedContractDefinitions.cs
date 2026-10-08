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
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBlueDoorVisualCatalog.cs", "F8253FFD84E9B420B8C2DFDD41362D4C4D5267529746EC4684284D502F0900A4"),
             new("csharp/src/SuperMetroid.Core/Rooms/BlueDoorPlmDrawDefinitions.cs", "EB429CBF26BEABFEF685F387F755BA02846EE5BCE2739576808D04DCCE3DE616")],
            "The constructor requires sixteen unique known four-word authored frames and clones only changed artwork; stock visuals project calculated physical words. Four closed-cap pointers resolve to required opening frames through the guarded VisualSource mapping; all twenty supported pointers have four words. Unknown pointers are not accepted by aliasing. Collision and opening timing are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmColoredDoorVisualCatalog.cs", "BA079DCA87D7D4BE9B3DAD74B69E506AAB8FA4FD22F2B34BE1DC56A03193F757"),
             new("csharp/src/SuperMetroid.Core/Rooms/ColoredDoorPlmDrawDefinitions.cs", "F4CC7DADEF38A4C6A7B504A548FBEF1FBAF023D3DB98ED8FD8DC743240DA407B")],
            "The constructor requires all forty-eight unique known yellow/green/red frames with four visual words each, cloning only changed artwork. Stock values are calculated and identity preserves the original sorted one-run framing. GetWord guards the pointer and array bounds. Projectile filters, persistence and conversion to blue caps are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGreyDoorVisualCatalog.cs", "521000C2A01131F1D3D9B899B8B396F01318511793C96D697CA944752C04D9D8"),
             new("csharp/src/SuperMetroid.Core/Rooms/GreyDoorPlmDrawDefinitions.cs", "2A31BEE40CB24E8C6D9CAC4E2C47FCE174511CB829B4FF525CE19FC304D1DB6B")],
            "The constructor requires all twenty unique known grey/clear frames with exact four-word payloads and clones only changed artwork. Stock words project calculated physical definitions. GetWord guards pointer and cell bounds; identity preserves sorted one-run framing. Condition gates, timers and sounds are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEyeDoorVisualCatalog.cs", "BC9A8C7D6275F8584517F52125ACC11FECE16CA867DAE3A0E1604B8ADDBC96C4"),
             new("csharp/src/SuperMetroid.Core/Rooms/EyeDoorPlmDrawDefinitions.cs", "2437AC5D28BF109E0FF797FDAAFC8BDC53512363AFE8AB15B26E5DC984824435")],
            "The constructor requires twenty-three unique authored frames with exact one/two/four-word payloads and clones only changed artwork. Stock words project calculated physical definitions. The twenty-fourth pointer uses the required four-word clear frame with horizontal flip. GetWord guards the mapped frame's calculated bounds; known narrow components cannot borrow clear-frame width. Eye attacks and conversion timing are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEscapeGateVisualCatalog.cs", "CCC9C3BB407AAC9F1A32633D3C70E94AAA1994D2DCEEA618827EBD96904BB29D"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainEscapeGatePlmDrawDefinitions.cs", "A020A85763B93F69C7292FEF49EE3E086382DAD5F894CD7D4522434F91EF7036")],
            "The constructor requires all three unique known escape-gate frames with exact cloned four-word payloads. GetWord guards the pointer and array bounds. Escape progression, physical words and animation cadence are not altered."),
        new("SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleVisualCatalog.cs", "B19701FC4E7294130A8AF11C6517158034C7D45A1A335B40503E44EA3183892C"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleDrawDefinitions.cs", "662600481B44367430E07ABAEB217EA8D826FF6074D5008B2CBAC7789C2C08B7")],
            "The constructor requires all twenty-four unique known collectible frame identities and retains only changed visual words. Stock words are calculated from physical draw definitions; GetWord guards membership in the complete pointer set. Pickup effects, graphics allocation and item persistence are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockVisualCatalog.cs", "78A2071CD7F82E26BC4F18394E82454AB0E5F5A24A6DB7EF727A6EA130F25C63"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockDrawDefinitions.cs", "1FBE71AE2509E5F452C106E9EE7B7ADA322A61B8AE87C6E4FC32C4CE5CFD0858")],
            "The constructor requires all five unique known Grapple-block frame pointers and stores independent visual words. GetWord guards that complete set. Grapple collision, breaking and respawn mechanics remain outside this resource proof."),
    ];
}
