using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's escape-gate block draws: which PLM draws are the gate's, their cells from
/// <see cref="MotherBrainEscapeGatePlmDrawDefinitions"/>.
/// </summary>
internal static class MotherBrainEscapeGateDraws
{
    /// <summary>Four vertical cells, always solid, selected by open/half/closed state.</summary>
    internal readonly record struct Draw(MotherBrainEscapeGateDraw Frame)
    {
        /// <summary>The terrain block of the gate's cell at <paramref name="row"/>, top to bottom.</summary>
        internal RoomTerrainBlock BlockAt(int row)
        {
            if ((uint)row >= 4) throw new ArgumentOutOfRangeException(nameof(row));
            return MotherBrainEscapeGatePlmDrawDefinitions.Cells[Frame][row];
        }
    }

    /// <summary>Recognizes one of the three gate draw pointers; other PLM draw families are left unclaimed.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        // Pointers outside the three gate images belong to other draw families.
        bool owned = request is PlmFrameDraw<MotherBrainEscapeGateDraw>;
        draw = owned ? new(PlmDraw.FrameOf<MotherBrainEscapeGateDraw>(request, "MotherBrainEscapeGateDraw")) : default;
        return owned;
    }
}
