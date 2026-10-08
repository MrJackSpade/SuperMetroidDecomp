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
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockVisualCatalog.cs", "3AB990375C828D052D2A5CAD40857275E0758496C4A1137CB86140E89C512229")],
            "The public constructor accepts only compiled draw identities with exact run/word shapes, rejects duplicate IDs and requires all nineteen lists. Stock runs use calculated draws; custom runs are cloned. GetWord guards pointer/run/word selections into that complete domain."),
        new("SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationVisualCatalog.cs", "3B25D17786322F458E5A83C9EC3830D625FD7CFFAE90C1532FDBCAA589BB6A15"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationDrawDefinitions.cs", "E03643585C6FAA35322C92DD4E0D44BF1181D55DCDA050042B96775B70704126")],
            "The constructor maps only compiled station IDs, requires each exact valid run/word shape and all twenty lists, rejects duplicates, and clones only changed frames. GetWord guards calculated pointer/run/word shapes and projects stock appearance from physical words; partial station art remains rejected."),
        new("SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs", "61366FBCFB6834BB95DE3A2C39E807B1AF968261AD808C52B3B221FE990C5BE8"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs", "FF6305F80AADFD65595FB6BFA3013054EC81D97C9F2A2DB145C23F95B0604456")],
            "The constructor requires both named hand frames, exact flattened word counts and unique compiled identities, then clones them. GetWord checks the compiled run/word shape before flattening its index; no instruction, debris or collision behavior is executed."),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs", "4AD4DCC29C4A9511DD1C71FF2E7C4EA0277904AE4C6B566275A7BA74061ABA0B"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs", "D3D3E703BD35DA242D5CE555FD32150D647317CA2C457ED6A241803C9EF13150")],
            "The constructor requires all eleven glass frames, unique compiled identities and exact flattened word counts, cloning only changed artwork. Stock visuals project the physical definitions. GetWord bounds-checks each native run/word selection before flattening; glass damage and shatter timing remain outside this proof."),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs", "06D30D7D0CFF33615AD9CE88BC535294C1E3C4B6F0102A2723C5D9C83160794E"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs", "50A37A644C4EA2A5BC4C446DF9BB70AC86594327E3EB5D51E97F6B7476697D5E")],
            "The constructor requires all seven tube frames with unique compiled identities and exact flattened payloads, cloning only custom frames. GetWord checks the calculated run/word shape and calculates unchanged stock visuals. Power-bomb gating, shards, events and liquid mechanics are not part of this resource-domain proof."),
    ];
}
