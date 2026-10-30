using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One room-PLM visual family as the shared door-visual codec loads it (#633): its JSON file name,
/// the family name used in diagnostics, the compiled draw lists it covers, how a draw list is
/// named in JSON, and how its catalog is built and read back for the parity check: the appearance the catalog shows
/// at a draw's run and block.
/// </summary>
internal sealed record RoomPlmVisualFamily<TCatalog>(
    string FileName,
    string Name,
    IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Definitions,
    Func<PlmDraw, string> VisualId,
    Func<RoomPlmDoorVisualFileCodec.Entry[], TCatalog> CreateCatalog,
    Func<TCatalog, PlmDraw, int, int, RoomBlockAppearance?> GetAppearance);
