
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One room-PLM visual family as the shared door-visual codec loads it (#633): its JSON file name,
/// the family name used in diagnostics, the compiled draw lists it covers, how a draw list is
/// named in JSON, and how its catalog is built and read back for the parity check: the appearance the catalog shows
/// at a draw's run and block.
/// </summary>
internal sealed record RoomObjectVisualFamily<TCatalog>(
    string FileName,
    string Name,
    Func<RoomObjectDoorVisualFileCodec.Entry[], TCatalog> CreateCatalog);
