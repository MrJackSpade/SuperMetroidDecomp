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
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs", "79BB5A2D752740013CBB9089C183769A6E8073E47F8782721AAC352F50FF22BE"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs", "B918E2F1F25F6A244F86BA34654B373755D14D94C4C6B0C5E052A463733C8887")],
            "The constructor accepts only compiled named frames, rejects duplicate IDs, requires all six gate-column frames and eight trigger draws with exact cloned run/word shapes. GetWord guards the complete pointer/run/word domain. Actor sprites and shot filters are outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs", "AC987409A86B886E5AAC54BC98D45A80BAB3282FB1F16F91829DD1BB1541DB62"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs", "EDB2E8CC9AB1F6AE42BF5049446174591081DD9AA110438D9AA45AF06BAB3298")],
            "The public constructor requires each of the three compiled draw identities with exact cloned run/word shapes and rejects duplicates. GetWord bounds-checks that complete domain. The four-step animation loop, camera and collision behavior are not executed or certified."),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs", "5505597E276B6FAD80531EC9045622F42A784BC6B80BE03A5CF3ED8BF5CEB8CF"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "3B0733D3260C6AD2E6B948AE5F2C56C236D2C658CA91E05DDF01ACF01BF62ACD")],
            "The constructor requires all twelve compiled left/right cannon frames, unique known IDs and exact flattened payloads, cloning only changed frames. GetWord guards calculated run/word shapes and projects stock appearance from calculated physical words. The unsupported diagonal orientations are not included or silently granted artwork."),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs", "5B4FD097115427748584B96E67A8103C2FE2FCF69DC2F50C370E44FB11CA105F"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs", "8472742279E40A1E081F658CAC49A43BAF56640EB7B3E72FA1CC862818D1D8F2")],
            "The constructor requires the cleared-hand and two slope-access layouts, rejects unknown/duplicate IDs and invalid flattened payloads, and clones only changed frames. GetWord checks calculated run/word shapes and projects stock visuals from calculated physical words. Statue movement and events remain outside this contract."),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs", "709B39F8171A88F837087D55A0FF061A5D71644B1E5C50473828B3C7BB12CEB2"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs", "32E489B25ED831F550E49FCDE454E87DEDFEAACC17CA94DDBA566D777775C9CD"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs", "B5EB8FBB21C0D16F5EFA61E8C2027B9D5393A2FD27F52CD1755A97B96622A3E9"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs", "46D84E659D58BE9F4F1E94A519B125090B26626263AAEA8A30C97A629452A050")],
            "The constructor requires all six bomb/contact-crumble restoration layouts, rejects unknown/duplicate IDs and clones exact flattened words. GetWord checks native run/word shapes. Both underlying definition sets are source-guarded; linked collision ownership is unchanged."),
    ];
}
