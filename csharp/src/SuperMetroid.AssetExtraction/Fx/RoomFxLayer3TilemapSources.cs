using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Where the cartridge stores each room effect's layer-three page and palette blend (#1415).</summary>
internal static class RoomFxLayer3TilemapSources
{
    /// <summary>
    /// The page <paramref name="type"/> shows: the six pages are consecutive in <see cref="RoomFxLayer3TilemapFormat.Types"/>
    /// order. The statue effect shows the water page; every other type has none.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="type"/> shows no page.</exception>
    internal static SnesAddress Page(RoomFxType type)
    {
        if (RoomFxTypes.UsesWater(type)) type = RoomFxType.Water;
        for (int page = 0; page < RoomFxLayer3TilemapFormat.Types.Count; page++)
            if (RoomFxLayer3TilemapFormat.Types[page] == type)
                return RoomFxLayer3SourceDefinitions.Layer3TilemapPages.AddWithinBank(page * RoomFxLayer3TilemapFormat.PageByteCount);
        throw new InvalidDataException($"Room-FX type {type} has no BG3 tilemap page.");
    }

    /// <summary>The first of <paramref name="id"/>'s three adjacent blend colours: the record's selector byte is an offset into the colour table.</summary>
    /// <exception cref="InvalidDataException"><paramref name="id"/> is not a retail blend.</exception>
    internal static SnesAddress PaletteBlend(RoomFxPaletteBlend id)
    {
        if (!RoomFxPaletteBlendDefinitions.Ids.Contains(id))
            throw new InvalidDataException($"Room-FX palette blend {id} is not a retail selector.");
        return RoomFxLayer3SourceDefinitions.PaletteBlendColors.AddWithinBank(RoomFxRecordWords.PaletteBlends.Word(id));
    }
}
