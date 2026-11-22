using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the arm cannon's twelve cover pictures (#1411, #2556): each orientation's opening in turn, one
/// character every $200 bytes of bank $9A from <see cref="ArmCannonTileSourceDefinitions.FirstTileSource"/>. This is the
/// only place a cover picture meets the bank-$9A source a queued transfer names it by, and the order the import's cover
/// page holds the pictures in.
/// </summary>
internal static partial class ArmCannonTileSources
{
    /// <summary>The bytes between one cover picture's source and the next.</summary>
    private const int TileSourceStride = 512;

    /// <summary>The orientations in the order the cartridge keeps their openings.</summary>
    private static readonly SamusArmCannonCoverOrientation[] OrientationOrder =
    [
        SamusArmCannonCoverOrientation.Vertical, SamusArmCannonCoverOrientation.Horizontal,
        SamusArmCannonCoverOrientation.DownwardDiagonal, SamusArmCannonCoverOrientation.UpwardDiagonal,
    ];

    /// <summary>Every cover picture in the order the cartridge keeps them: each orientation's opening, frame by frame.</summary>
    internal static IReadOnlyList<SamusArmCannonCoverTile> InSourceOrder { get; } =
    [
        .. OrientationOrder.SelectMany(orientation => SamusArmCannonCoverFrameCodec.InOpeningOrder
            .Select(frame => new SamusArmCannonCoverTile(orientation, frame))),
    ];

    /// <summary>Each cover picture's bank-$9A source.</summary>
    private static readonly FrozenDictionary<SamusArmCannonCoverTile, ushort> Sources = InSourceOrder
        .Select((tile, place) => KeyValuePair.Create(tile, checked((ushort)(ArmCannonTileSourceDefinitions.FirstTileSource + place * TileSourceStride))))
        .ToFrozenDictionary();

    /// <summary>The cover picture at each bank-$9A source.</summary>
    private static readonly FrozenDictionary<int, SamusArmCannonCoverTile> Tiles =
        Sources.ToFrozenDictionary(entry => (int)entry.Value, entry => entry.Key);

    /// <summary>The bank-$9A source of <paramref name="tile"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tile"/> is no cover picture.</exception>
    internal static ushort Source(SamusArmCannonCoverTile tile) => Sources.TryGetValue(tile, out ushort source)
        ? source
        : throw new ArgumentOutOfRangeException(nameof(tile), tile, "No cover picture is kept there.");

    /// <summary>The cover picture whose bank-$9A source is <paramref name="source"/>, if one's is.</summary>
    internal static bool TryDecode(int source, out SamusArmCannonCoverTile tile) => Tiles.TryGetValue(source, out tile);

    /// <summary>Each cover picture's characters on <paramref name="page"/>, which holds them in source order.</summary>
    /// <exception cref="InvalidDataException">The page does not hold one character of each cover picture.</exception>
    internal static FrozenDictionary<SamusArmCannonCoverTile, ReadOnlyMemory<byte>> SplitPage(ReadOnlyMemory<byte> page)
    {
        int bytes = SamusArmCannonArtworkFormat.CoverTileByteCount;
        if (page.Length != InSourceOrder.Count * bytes)
            throw new InvalidDataException("Arm-cannon cover characters must be the twelve cover pictures.");
        return InSourceOrder.Select((tile, place) => KeyValuePair.Create(tile, page.Slice(place * bytes, bytes))).ToFrozenDictionary();
    }
}
