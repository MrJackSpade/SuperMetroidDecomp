using System.Collections;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>$82:CAE9-CBCA: three turning helmet drawings followed by five visor glints.
/// Shared atlas packing and overlapping part coordinates calculate; selected drawings, native part order and visor alignment are retained as this exact display design. Pixels, colors, anchors and timing are excluded.</summary>
internal sealed class FileSelectHelmetParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>$82:CAE9: first 24x24 helmet block begins at tileD0; three adjacent three-tile-wide poses share its atlas rows.</summary>
    private const int FirstBodyTile = FileSelectPresentationDefinitions.HelmetFirstBodyCell;
    /// <summary>$82:CB2B: first two-tile visor strip begins atD9, with consecutive pairs packed into available atlas columns.</summary>
    private const int FirstVisorTile = FileSelectPresentationDefinitions.HelmetFirstVisorCell;
    /// <summary>$82:CAE9-CBCA: each body pose is three8pixel tiles square, composed from four overlapping16pixel OBJs.</summary>
    private const int BodyTileSide = 3;
    /// <summary>$82:CB2B-CBCA: selected visor overlay top lies two pixels above the shared helmet anchor.</summary>
    private const int VisorTop = -2;
    /// <summary>One character tile spans eight pixels in the menu atlas.</summary>
    private const int TileSide = 8;
    /// <summary>Large OAM parts cover sixteen pixels and overlap in the 24-pixel helmet.</summary>
    private const int LargeSide = 16;
    /// <summary>Four overlapping OAM placements compose each helmet body pose.</summary>
    private enum Corner
    {
        /// <summary>Upper-left overlapping body part.</summary>
        TopLeft,
        /// <summary>Upper-right overlapping body part.</summary>
        TopRight,
        /// <summary>Lower-left overlapping body part.</summary>
        BottomLeft,
        /// <summary>Lower-right overlapping body part.</summary>
        BottomRight
    }
    /// <summary>Replaces matching Helmet.0 through Helmet.7 compositions with reviewed part geometry.</summary>
    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied) =>
        name.Length == 8 && name.StartsWith("Helmet.", StringComparison.Ordinal) && name[7] is >= '0' and <= '7'
            ? supplied.CalculateIfMatching(new FileSelectHelmetParts(name[7] - '0')) : supplied;
    /// <summary>Turning poses use four body parts; visor poses add two small glint parts.</summary>
    public int Count => frame < 3 ? 4 : 6;
    /// <summary>Reorders overlapping body corners for each of the three turning poses.</summary>
    private Corner BodyCorner(int part) => frame switch
    {
        0 => part switch { 0 => Corner.TopRight, 1 => Corner.BottomRight, 2 => Corner.BottomLeft, _ => Corner.TopLeft },
        1 => part switch { 0 => Corner.BottomLeft, 1 => Corner.TopLeft, 2 => Corner.TopRight, _ => Corner.BottomRight },
        _ => part switch { 0 => Corner.TopRight, 1 => Corner.TopLeft, 2 => Corner.BottomLeft, _ => Corner.BottomRight },
    };
    /// <summary>Places one visor glint or overlapping helmet body part at its reviewed tile and offset.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (frame >= 3 && index < 2)
            {
                int phase = frame - 3;
                int pairsPerRow = (MapSpriteFormat.TileColumns - FirstVisorTile % MapSpriteFormat.TileColumns) / 2;
                int tile = FirstVisorTile + phase % pairsPerRow * 2 + phase / pairsPerRow * MapSpriteFormat.TileColumns;
                bool right = index == 0;
                return new(SnesSpritemapXWord.Create(right ? 0 : -TileSide, false), SnesSpritemapYByte.Create(VisorTop),
                    SnesObjAttributeWord.Create(tile + (right ? 1 : 0), 0, 3, default), true);
            }
            Corner corner = BodyCorner(index - (frame >= 3 ? 2 : 0));
            int x = corner is Corner.TopRight or Corner.BottomRight ? 1 : 0;
            int y = corner is Corner.BottomLeft or Corner.BottomRight ? 1 : 0;
            int side = BodyTileSide * TileSide, step = side - LargeSide;
            int bodyTile = FirstBodyTile + Math.Min(frame, 2) * BodyTileSide + x + y * MapSpriteFormat.TileColumns;
            return new(SnesSpritemapXWord.Create(-side / 2 + x * step, true), SnesSpritemapYByte.Create(-side / 2 + y * step),
                SnesObjAttributeWord.Create(bodyTile, 0, 3, default), true);
        }
    }
    /// <summary>Yields visor overlays before body parts where the native composition does.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same ordered parts to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
