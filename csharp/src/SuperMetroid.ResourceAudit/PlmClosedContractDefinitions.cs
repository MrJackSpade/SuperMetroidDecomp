namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockVisualCatalog.cs", "E56C186ECC44176D0299C5C8A3525DCBE8B35F7CA8A9EBC80BD756571C5F7339")],
            "The public constructor accepts only compiled draw identities with exact run/word shapes, rejects duplicate IDs and requires all nineteen lists. Stock runs use calculated draws; custom runs are cloned. GetWord guards pointer/run/word selections into that complete domain."),
        new("SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationVisualCatalog.cs", "AA88367B7690491545A539165C933B561E1E02698391F9EC57259ACF6395EB99"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationDrawDefinitions.cs", "072E431B17003A3FF093957E6F1B5E6FF9637B06E1884B11A3DEA6E26C125B6D")],
            "The constructor maps only compiled station visual IDs, requires each exact run/word shape, rejects duplicates and requires every station list before cloning it. GetWord has pointer/run/word guards; the provider cannot publish partial valid station art."),
        new("SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs", "F7338AF3840D2956C1913A1D97FE587A8E26124E4BF369AF6A758A89B30F0C56"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs", "FF6305F80AADFD65595FB6BFA3013054EC81D97C9F2A2DB145C23F95B0604456")],
            "The constructor requires both named hand frames, exact flattened word counts and unique compiled identities, then clones them. GetWord checks the compiled run/word shape before flattening its index; no instruction, debris or collision behavior is executed."),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs", "06943B286EC25BC003C6DC27B2E24BF404924BD418F32CD5BC1A72A5A2BB70DC"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs", "C8F25F652B354EC8655F83A71F7E3B6DFD3B90DEF0E69EF6BFABCC916B1A6D2C")],
            "The constructor requires all eleven glass frames, unique compiled identities and exact flattened word counts before cloning. GetWord bounds-checks each native run/word selection before flattening; glass damage and shatter timing remain outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs", "9FCE1ABB071EF40C17B2557D6577E6AFA74118A90AFC65FD8BBBE226A2F716BA"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs", "CAA6402C381FD6BE459AAF9569DE3178E3BC4FB86197E23B7B8D0924129B56A4")],
            "The constructor requires all seven tube frames with unique compiled identities and exact cloned flattened payloads. GetWord checks the declared run/word shape. Power-bomb gating, shards, events and liquid mechanics are not part of this resource-domain proof."),
    ];
}
