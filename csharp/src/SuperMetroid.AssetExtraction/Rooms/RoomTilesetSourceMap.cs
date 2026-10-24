using System.Collections.Frozen;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where the cartridge stores each room character sheet, block table and palette.</summary>
/// <param name="Characters">Each character sheet's source; the Tourian statue ghost's is stored uncompressed.</param>
/// <param name="Blocks">Each block table's compressed source.</param>
/// <param name="Palettes">Each palette's compressed source.</param>
internal sealed record RoomTilesetSourceMap(
    FrozenDictionary<RoomCharacterSet, SnesAddress> Characters,
    FrozenDictionary<RoomBlockSet, SnesAddress> Blocks,
    FrozenDictionary<RoomPaletteSet, SnesAddress> Palettes);
