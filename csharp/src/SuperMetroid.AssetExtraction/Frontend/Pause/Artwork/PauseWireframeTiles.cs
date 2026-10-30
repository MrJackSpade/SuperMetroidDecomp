using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseWireframeDefinitions;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Draws the equipment screen's Samus wireframes (<see cref="PauseWireframeDefinitions"/>) from their shared pieces.
/// </summary>
public static class PauseWireframeTiles
{
    /// <summary>Compose one cell of the four authored equipment diagrams at $82:D521/D631/D741/D851.</summary>
    /// <remarks>The thirty named pieces, placements, variant membership and layering specify these
    /// drawings. Atlas strips, shared knee/foot pieces and reflections calculate their repeated cells.
    /// Connector palette six versus two is preserved copied metadata: stock colors coincide, but
    /// independently edited palettes can distinguish them. This does not exempt the glyph pixels.</remarks>
    internal static BackgroundTile StockTile(PauseWireframeKind kind, int cell)
    {
        bool piece = TryStockTile(kind, cell, out int tile);
        int row = cell / Columns, column = cell % Columns;
        bool varia = kind is PauseWireframeKind.VariaSuit or PauseWireframeKind.VariaSuitHiJump;
        bool hiJump = kind is PauseWireframeKind.PowerSuitHiJump or PauseWireframeKind.VariaSuitHiJump;
        if (row == 9 && column == 6) return default; // Cannon-side negative space.
        // The diagonal connector is drawn in front and mirrored both ways.
        if (row == 14 && column == 7)
            return new BackgroundTile(DiagonalConnector, kind is PauseWireframeKind.PowerSuit or PauseWireframeKind.VariaSuitHiJump ? 6 : 2,
                highPriority: true, TileFlip.Horizontal | TileFlip.Vertical);
        if (!piece)
        {
            if (!varia && row == 6 && column == 2) tile = PowerTorso - PieceRowStride;
            else if (row == 9 && column == 1) tile = HandTip;
            else if (!varia && !hiJump && row == 14 && column == 2) tile = RegularPowerAnkle;
            else if (!hiJump && row == 15 && column == 0) tile = RegularToeEdge;
            else if (!varia && hiJump && row == 13 && column == 2) tile = PowerHiJumpInnerCollar;
            else if (varia && !hiJump && row == 13 && column == 3) tile = PowerLeg + 2 * PieceRowStride + 1;
            else if (row == 15 && column == 7) tile = HiJumpOuterFoot + 1;
            else if (varia && !hiJump && row == 16 && column == 7) tile = RegularFootOuter + PieceRowStride + 1;
            else tile = -1;
        }
        BackgroundTile placed;
        if (tile >= 0) placed = new BackgroundTile(tile, CommonPiecePalette, highPriority: true);
        else if (column < Columns / 2) placed = default;
        else
        {
            BackgroundTile paired = StockTile(kind, row * Columns + Columns - 1 - column);
            placed = paired == default ? default : paired.WithFlipsToggled(TileFlip.Horizontal);
        }
        bool underlay = (row == 13 && column == 3 && !(varia && hiJump))
            || (!varia && hiJump && column == 1 && row is 13 or 14)
            || (hiJump && column == 3 && row == 15)
            || (varia && !hiJump && column is 1 or 4 && row is 11 or 12);
        if (underlay) placed = placed with { HighPriority = false };
        if (varia && !hiJump && column == 6 && row is 11 or 12) placed = placed with { HighPriority = true };
        return placed;
    }
    /// <summary>Calculate tile references within identified rectangular body pieces.</summary>
    /// <remarks>The named piece composition is authored drawing content; StockTile completes its
    /// shared glyphs and layering. This strip-only operation also serves the separately
    /// accounted equipment base page.</remarks>
    internal static bool TryStockTile(PauseWireframeKind kind, int cell, out int tile)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if ((uint)cell >= Cells) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / Columns, column = cell % Columns;
        bool varia = kind is PauseWireframeKind.VariaSuit or PauseWireframeKind.VariaSuitHiJump;
        bool hiJump = kind is PauseWireframeKind.PowerSuitHiJump or PauseWireframeKind.VariaSuitHiJump;
        if (row <= 2 && column == 3) tile = Helmet + row * PieceRowStride;
        else if ((row is >= 1 and <= 2 && column < 3) || (!varia && row == 3 && column < 2))
            tile = (varia ? VariaShoulder : PowerShoulder) + (row - 1) * PieceRowStride + column;
        else if ((row is >= 5 and <= 8 && column < 2) || (row == 5 && column < 5) || (row == 6 && column is >= 3 and <= 4))
            tile = Arm + (row - 5) * PieceRowStride + column;
        else if (varia && row == 3) tile = VariaChest + column;
        else if (!varia && row == 3 && column is 2 or 3) tile = PowerChestLeft + column - 2;
        else if (!varia && row == 3 && column >= 4) tile = PowerChestRight + column - 4;
        else if (row == 4 && column < 2) tile = LowerChestLeft + column;
        else if (row == 4 && column is 2 or 3) tile = (varia ? VariaLowerChest : PowerLowerChest) + column - 2;
        else if (row is 6 or 7 && column == 5)
            tile = (varia ? VariaTorso + column - 2 : CannonInner) + (row - 6) * PieceRowStride;
        else if (row is >= 6 and <= 8 && column >= 6) tile = CannonOuter + (row - 6) * PieceRowStride + column - 6;
        else if (varia && row is >= 6 and <= 8 && column is >= 2 and <= 3)
            tile = VariaTorso + (row - 6) * PieceRowStride + column - 2;
        else if (!varia && row is >= 7 and <= 8 && column is >= 2 and <= 3)
            tile = PowerTorso + (row - 7) * PieceRowStride + column - 2;
        else if (row is >= 9 and <= 10 && column is >= 2 and <= 3)
            tile = (varia ? VariaHip : PowerHip) + (row - 9) * PieceRowStride + column - 2;
        else if (varia && row is >= 11 and <= 12 && column is >= 1 and <= 3)
            tile = VariaLeg + (row - 11) * PieceRowStride + column - 1;
        else if (!varia && row is >= 11 and <= 13 && column is >= 2 and <= 3 &&
                 (row < 13 || !hiJump || column == 3))
            tile = PowerLeg + (row - 11) * PieceRowStride + column - 2;
        else if (hiJump && row is >= 15 and <= 16 && (column < 3 || (row == 15 && column == 3)))
            tile = HiJumpFoot + (row - 15) * PieceRowStride + column;
        else if (hiJump && row is >= 15 and <= 16 && column == 6)
            tile = HiJumpOuterFoot + (row - 15) * PieceRowStride;
        else if (varia && !hiJump && row is 13 or 14 && column == 2)
            tile = VariaAnkle + (row - 13) * PieceRowStride;
        else if (hiJump && row is 13 or 14 && column is 1 or 2 &&
                 (varia || row == 14 || column == 1))
            tile = HiJumpCollar + (row - 13) * PieceRowStride + column - 1;
        else if (!hiJump && row == 15 && column is 1 or 2) tile = RegularFootUpper + column - 1;
        else if (!hiJump && row == 16 && column < 3) tile = RegularFootToe + column;
        else if (!hiJump && row is 15 or 16 && column == 6)
            tile = RegularFootOuter + (row - 15) * PieceRowStride;
        else { tile = 0; return false; }
        return true;
    }
}
