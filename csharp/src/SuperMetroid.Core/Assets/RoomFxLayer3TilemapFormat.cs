using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Native bank-$8A 32x33 BG3 effect-page geometry and presentation identities.</summary>
public static class RoomFxLayer3TilemapFormat
{
    /// <summary>Shared editable JSON filename containing all six room-FX BG3 page definitions.</summary>
    public const string FileName = "room-fx-layer3-tilemaps.json";
    /// <summary>Required revision of the tile-reference document schema, independent of the native FX type identifiers.</summary>
    public const int Version = 1;
    /// <summary>Destination cells per transfer row; cell index modulo 32 selects its map column, not its character-sheet column.</summary>
    public const int WidthInTiles = 32;
    /// <summary>Rows retained from each native transfer, including its thirty-third row; these resources exceed a standard 32-by-32 BG page.</summary>
    public const int HeightInTiles = 33;
    /// <summary>Required editable cells per FX resource, 1056, preserving the complete native 32-by-33 transfer.</summary>
    public const int CellsPerPage = WidthInTiles * HeightInTiles;
    /// <summary>Compiled transfer length, $0840 bytes, with one little-endian sixteen-bit SNES BG tilemap word per cell.</summary>
    public const int PageByteCount = CellsPerPage * sizeof(ushort);

    /// <summary>$8A:8000, first room-FX BG3 page; the six pages end at $8A:B17F.</summary>
    public const int FirstSourceAddress = SuperMetroid.Core.Game.RoomFxRomData.Layer3TilemapPages;

    /// <summary>
    /// The six even type identities $02..0C at $83:ABF2..ABFC, in page order.
    /// Index0..5 names Lava, Acid, Water, Spores, Rain and Fog in page order.
    /// </summary>
    public static IReadOnlyList<RoomFxType> Types { get; } = new CalculatedTypes();

    /// <summary>Exposes the six native room-FX page identities in transfer order without storing an array.</summary>
    private sealed class CalculatedTypes : IReadOnlyList<RoomFxType>
    {
        /// <summary>Six FX types own authored BG3 pages; the statue effect aliases water.</summary>
        public int Count => 6;
        /// <summary>Maps a transfer-page index to its lava-through-fog effect type.</summary>
        public RoomFxType this[int index] => index switch
        {
            0 => RoomFxType.Lava,
            1 => RoomFxType.Acid,
            2 => RoomFxType.Water,
            3 => RoomFxType.Spores,
            4 => RoomFxType.Rain,
            5 => RoomFxType.Fog,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        /// <summary>Walks the six page identities in their bank-$8A transfer order.</summary>
        public IEnumerator<RoomFxType> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }

        /// <summary>Provides the same ordered page traversal through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Returns $8A:8000 + (type/2-1)*$840 for the six even types $02..0C.
    /// The original pointers at $83:ABF2..ABFC select consecutive 32x33 word pages.
    /// The statue effect aliases water; every other ushort type remains unsupported.
    /// </summary>
    public static int SourceAddress(RoomFxType type)
    {
        if (RoomFxTypes.UsesWater(type)) type = RoomFxType.Water;
        int value = (ushort)type;
        if (value is < 2 or > 12 || (value & 1) != 0)
            throw new InvalidDataException($"Room-FX type {type} has no BG3 tilemap page.");
        return FirstSourceAddress + (value / 2 - 1) * PageByteCount;
    }
}
