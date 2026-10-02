namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs", "828BCFD646AB89659A548EA2E6597D16D5CFBAD1016DFB31C952548C03DA5961"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs", "52DB7900C127F7046C761CC255471A20A5B2AA1B21BF0E970FCBE2E1635AA004")],
            "The constructor accepts only compiled named frames, rejects duplicate IDs, requires all six gate-column frames and eight trigger draws with exact cloned run/word shapes. GetWord guards the complete pointer/run/word domain. Actor sprites and shot filters are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs", "AC987409A86B886E5AAC54BC98D45A80BAB3282FB1F16F91829DD1BB1541DB62"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs", "EDB2E8CC9AB1F6AE42BF5049446174591081DD9AA110438D9AA45AF06BAB3298")],
            "The public constructor requires each of the three compiled draw identities with exact cloned run/word shapes and rejects duplicates. GetWord bounds-checks that complete domain. The four-step animation loop, camera and collision behavior are not executed or certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs", "5EF62FCEDA7C93E2EF756BFE2E5390903EB324278B91650B497AEE6B7F228457"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "ED5F27E41CBEF7CC7AE0124E69A1DAF29E79843F98DCCD2D0C1BAE2E56658254")],
            "The constructor requires all twelve compiled left/right cannon frames, unique known IDs and exact cloned flattened payloads. GetWord guards the native run/word shapes. The unsupported diagonal orientations are not included or silently granted artwork."),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs", "7C83B4A7944647882AA2C61BEFA2A1BC1E88CB0BEC468365C055A412CF89DC6A"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs", "F20CDE0C1AC0C65F4852AF0E1409801003E390B2C30F69F5329C0C34C0A01E44")],
            "The constructor requires the cleared-hand and two slope-access layouts, rejects unknown/duplicate IDs and clones exact flattened payloads. GetWord checks each native run/word shape before flattening. Statue movement, events and physical geometry are not modified."),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs", "499F0C96F9A44C1D3C31BD0115509385A420575083476D008358F3233539C3C9"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs", "32E489B25ED831F550E49FCDE454E87DEDFEAACC17CA94DDBA566D777775C9CD"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs", "04E75B07DA3DBD79D012D3143CE8E2A66270D3987FE4FBD7D5680DB5068304DD"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs", "46D84E659D58BE9F4F1E94A519B125090B26626263AAEA8A30C97A629452A050")],
            "The constructor requires all six bomb/contact-crumble restoration layouts, rejects unknown/duplicate IDs and clones exact flattened words. GetWord checks native run/word shapes. Both underlying definition sets are source-guarded; linked collision ownership is unchanged."),
    ];
}
