using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's hand block draws: which PLM draws are the hand's, the strips each lays from
/// <see cref="BombTorizoHandPlmDrawDefinitions"/>, and their artwork identities.
/// </summary>
internal static class BombTorizoHandDraws
{
    /// <summary>
    /// An intact or cleared hand draw. Original run order and origin-relative continuations are preserved.
    /// </summary>
    internal readonly record struct Draw(BombTorizoHandDraw Frame)
    {
        /// <summary>The draw's strips in native run order.</summary>
        private BombTorizoHandStrip[] Strips => BombTorizoHandPlmDrawDefinitions.Strips[Frame];
        /// <summary>Gets the number of horizontal strips in the selected native draw list.</summary>
        internal int RunCount => Strips.Length;
        /// <summary>Gets the block count in a selected strip, rejecting an invalid run index.</summary>
        internal int BlockCount(int run) => Strip(run).Cells.Length;
        /// <summary>Gets a strip's horizontal origin relative to the hand PLM block.</summary>
        internal sbyte OriginX(int run) => checked((sbyte)Strip(run).Origin.X);
        /// <summary>Gets a strip's vertical origin relative to the hand PLM block.</summary>
        internal sbyte OriginY(int run) => checked((sbyte)Strip(run).Origin.Y);
        /// <summary>Gets the next strip's relative X origin or zero at list termination.</summary>
        internal sbyte NextX(int run)
        {
            _ = Strip(run);
            return run + 1 == RunCount ? (sbyte)0 : OriginX(run + 1);
        }
        /// <summary>Gets the next strip's relative Y origin or zero at list termination.</summary>
        internal sbyte NextY(int run)
        {
            _ = Strip(run);
            return run + 1 == RunCount ? (sbyte)0 : OriginY(run + 1);
        }
        /// <summary>Gets one strip cell's terrain block.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            BombTorizoHandStrip strip = Strip(run);
            if ((uint)cell >= strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell];
        }
        /// <summary>Selects a strip, rejecting indices outside the selected intact or cleared list.</summary>
        private BombTorizoHandStrip Strip(int run) =>
            (uint)run < RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Recognizes one of the two native hand draw pointers and returns its calculated descriptor.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        bool owned = request is PlmFrameDraw<BombTorizoHandDraw>;
        draw = owned ? new(PlmDraw.FrameOf<BombTorizoHandDraw>(request, "BombTorizoHandDraw")) : default;
        return owned;
    }

    /// <summary>Enumerates temporary draw DTOs for intact and cleared hand artwork in native order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            TryGet(PlmDraw.Of(BombTorizoHandDraw.Intact), out var intact);
            yield return intact;
            TryGet(PlmDraw.Of(BombTorizoHandDraw.Cleared), out var cleared);
            yield return cleared;
        }
    }

    /// <summary>Expands a calculated hand draw into an import/export DTO; gameplay computes its strips directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(request, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[draw.BlockCount(run)];
            for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = draw.BlockAt(run, cell);
            runs[run] = new(PlmRunDirection.Horizontal, blocks, new(draw.NextX(run), draw.NextY(run)));
        }
        list = new(request, runs);
        return true;
    }

    /// <summary>Gets the stable artwork ID for a supported native hand draw pointer.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<BombTorizoHandDraw>(request, "Bomb Torizo hand draw with a visual ID"));

    /// <summary>Maps an intact or cleared hand draw to its stable artwork ID.</summary>
    internal static string VisualId(BombTorizoHandDraw frame) =>
        BombTorizoHandPlmDrawDefinitions.VisualIds.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined BombTorizoHandDraw {frame}.");

    /// <summary>Resolves an ordinal artwork ID to the corresponding temporary hand draw DTO.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out list);
}
