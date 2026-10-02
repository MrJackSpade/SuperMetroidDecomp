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
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs", "9B910CCD8ABC600FC53392FC860DCFB1267215C31AD4417269B6AC849115781B"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs", "FF6305F80AADFD65595FB6BFA3013054EC81D97C9F2A2DB145C23F95B0604456")],
            "The constructor requires both named hand frames, exact flattened word counts and unique compiled identities, then clones them. GetWord checks the compiled run/word shape before flattening its index; no instruction, debris or collision behavior is executed."),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs", "0B9003A7B9FFA320527508144E124C5CDB67D0580318293BA8596579A345C125"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs", "E57659DF4BBDD19B3B65F46D81D784D9E5CCAE719988750DF33A9FF79641D940")],
            "The constructor requires all eleven glass frames, unique compiled identities and exact flattened word counts, cloning only changed artwork. Stock visuals project the physical definitions. GetWord bounds-checks each native run/word selection before flattening; glass damage and shatter timing remain outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs", "28793D14E283207822071158031EAE5CC02C76F7144436ABAA85C64DE360D5E4"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs", "50A37A644C4EA2A5BC4C446DF9BB70AC86594327E3EB5D51E97F6B7476697D5E")],
            "The constructor requires all seven tube frames with unique compiled identities and exact flattened payloads, cloning only custom frames. GetWord checks the calculated run/word shape and calculates unchanged stock visuals. Power-bomb gating, shards, events and liquid mechanics are not part of this resource-domain proof."),
    ];
}
