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
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBlueDoorVisualCatalog.cs", "F65EC90050ECB86745604F22A7D8D62C0C1B9E74E4FC2BBC17F460EA23DF7AEB"),
             new("csharp/src/SuperMetroid.Core/Rooms/BlueDoorPlmDrawDefinitions.cs", "307447692C701C172A97F876BE3D5D5087B7369950E0C3E80E86860389ECA3B4")],
            "The constructor requires sixteen unique known four-word authored frames and clones only changed artwork; stock visuals project calculated physical words. Four closed-cap pointers resolve to required opening frames through the guarded VisualSource mapping; all twenty supported pointers have four words. Unknown pointers are not accepted by aliasing. Collision and opening timing are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmColoredDoorVisualCatalog.cs", "5E7C428A162FF6E41F7E39935953BD4F283829925B753A3D18C2A5B207B0E8F7"),
             new("csharp/src/SuperMetroid.Core/Rooms/ColoredDoorPlmDrawDefinitions.cs", "2E52D5D348728190CC583580F9CE9E4F0FAFC24CCF6056390CB72836121992F8")],
            "The constructor requires all forty-eight unique known yellow/green/red frames with four visual words each, cloning only changed artwork. Stock values are calculated and identity preserves the original sorted one-run framing. GetWord guards the pointer and array bounds. Projectile filters, persistence and conversion to blue caps are unchanged."),
        new("SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGreyDoorVisualCatalog.cs", "B7CA5DFCF0DD8A32514E01698F373031B91D81AFC0F649F8CAB69A6982F217CC"),
             new("csharp/src/SuperMetroid.Core/Rooms/GreyDoorPlmDrawDefinitions.cs", "52B836170AD73B5000E3A1895C5BA9A1A5DA2E429EDDC7E0FD097E557B3B3B83")],
            "The constructor requires all twenty unique known grey/clear frames with exact four-word payloads and clones only changed artwork. Stock words project calculated physical definitions. GetWord guards pointer and cell bounds; identity preserves sorted one-run framing. Condition gates, timers and sounds are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEyeDoorVisualCatalog.cs", "EB77B3DA5FEAE614BC8408C4C4864B191664BD301244B966AD22DC22C92A2DDA"),
             new("csharp/src/SuperMetroid.Core/Rooms/EyeDoorPlmDrawDefinitions.cs", "2437AC5D28BF109E0FF797FDAAFC8BDC53512363AFE8AB15B26E5DC984824435")],
            "The constructor requires twenty-three unique authored frames with exact one/two/four-word payloads and clones only changed artwork. Stock words project calculated physical definitions. The twenty-fourth pointer uses the required four-word clear frame with horizontal flip. GetWord guards the mapped frame's calculated bounds; known narrow components cannot borrow clear-frame width. Eye attacks and conversion timing are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmEscapeGateVisualCatalog.cs", "44F12ED552662A3F5A66CB9EF61FF56E6F6F21B043D7F76122A4AF18186321AB"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainEscapeGatePlmDrawDefinitions.cs", "A020A85763B93F69C7292FEF49EE3E086382DAD5F894CD7D4522434F91EF7036")],
            "The constructor requires all three unique known escape-gate frames with exact cloned four-word payloads. GetWord guards the pointer and array bounds. Escape progression, physical words and animation cadence are not altered."),
        new("SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleVisualCatalog.cs", "EF64C05BF9EE9D884BEC0CB6B336BD2B610DC51ED3341D57EF63FC84F8C93473"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmCollectibleDrawDefinitions.cs", "662600481B44367430E07ABAEB217EA8D826FF6074D5008B2CBAC7789C2C08B7")],
            "The constructor requires all twenty-four unique known collectible frame identities and retains only changed visual words. Stock words are calculated from physical draw definitions; GetWord guards membership in the complete pointer set. Pickup effects, graphics allocation and item persistence are not certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockVisualCatalog.cs", "928B33A7F718A82B5523D2EACADDB4417222B5F05D2D94FEB816370F40BB6499"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmGrappleBlockDrawDefinitions.cs", "1FBE71AE2509E5F452C106E9EE7B7ADA322A61B8AE87C6E4FC32C4CE5CFD0858")],
            "The constructor requires all five unique known Grapple-block frame pointers and stores independent visual words. GetWord guards that complete set. Grapple collision, breaking and respawn mechanics remain outside this resource proof."),
    ];
}
