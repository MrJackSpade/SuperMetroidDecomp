using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator platform's block draws: which PLM draws are the platform's, the strips each frame lays from
/// <see cref="ElevatorPlatformObjectDefinitions"/>.
/// </summary>
internal static class ElevatorPlatformDraws
{
    /// <summary>One of the three platform frames' strips.</summary>
    internal readonly record struct Draw(ElevatorPlatformDraw Frame)
    {
        /// <summary>The frame's strips in order.</summary>
        private RoomObjectBlockStrip[] Strips => ElevatorPlatformObjectDefinitions.Strips[Frame];
        /// <summary>Gets the three strips.</summary>
        internal int RunCount => Strips.Length;
        /// <summary>Gets one edge cell or four lower-row cells for a selected strip.</summary>
        internal int BlockCount(int run) => Strip(run).Cells.Length;
        /// <summary>Where a strip starts, in blocks from the PLM origin.</summary>
        internal Offset16<RoomBlocks> Start(int run) => Strip(run).Start;
        /// <summary>Gets a solid platform block.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            RoomObjectBlockStrip strip = Strip(run);
            if ((uint)cell >= (uint)strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell];
        }
        /// <summary>Selects a strip, rejecting a strip index outside this platform draw.</summary>
        private RoomObjectBlockStrip Strip(int run) =>
            (uint)run < (uint)RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Recognizes one of the three platform frames.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        // Draws of other families are not the platform's.
        draw = request is RoomObjectFrameDraw<ElevatorPlatformDraw> { Frame: var platform }
            ? new(ElevatorPlatformObjectDefinitions.Strips.ContainsKey(platform)
                ? platform
                : throw new InvalidOperationException($"Undefined {nameof(ElevatorPlatformDraw)} {platform}."))
            : default;
        return request is RoomObjectFrameDraw<ElevatorPlatformDraw>;
    }
}
