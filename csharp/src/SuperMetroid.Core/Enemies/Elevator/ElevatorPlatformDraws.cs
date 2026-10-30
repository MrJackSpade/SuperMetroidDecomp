using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator platform's block draws: which PLM draws are the platform's, the strips each frame lays from
/// <see cref="ElevatorPlatformPlmDefinitions"/>, and their artwork identities.
/// </summary>
internal static class ElevatorPlatformDraws
{
    /// <summary>One of the three platform frames' strips.</summary>
    internal readonly record struct Draw(ElevatorPlatformDraw Frame) : IPlmRunDrawShape
    {
        /// <summary>The frame's strips in native order.</summary>
        private PlmBlockStrip[] Strips => ElevatorPlatformPlmDefinitions.Strips[Frame];
        /// <summary>Gets the three native strips.</summary>
        public int RunCount => Strips.Length;
        /// <summary>Gets one edge cell or four lower-row cells for a selected strip.</summary>
        public int BlockCount(int run) => Strip(run).Cells.Length;
        /// <summary>Gets the next strip's absolute horizontal offset from the PLM origin.</summary>
        public sbyte NextX(int run) => checked((sbyte)Strip(run).Next.X);
        /// <summary>Gets the next strip's absolute vertical offset from the PLM origin.</summary>
        public sbyte NextY(int run) => checked((sbyte)Strip(run).Next.Y);
        /// <summary>Gets a solid platform block.</summary>
        public RoomTerrainBlock BlockAt(int run, int cell)
        {
            PlmBlockStrip strip = Strip(run);
            if ((uint)cell >= (uint)strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell];
        }
        /// <summary>Selects a strip, rejecting a strip index outside this platform draw.</summary>
        private PlmBlockStrip Strip(int run) =>
            (uint)run < (uint)RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Recognizes one of the three platform frames.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        // Draws of other families are not the platform's.
        draw = request is PlmFrameDraw<ElevatorPlatformDraw> { Frame: var platform }
            ? new(ElevatorPlatformPlmDefinitions.Strips.ContainsKey(platform)
                ? platform
                : throw new InvalidOperationException($"Undefined {nameof(ElevatorPlatformDraw)} {platform}."))
            : default;
        return request is PlmFrameDraw<ElevatorPlatformDraw>;
    }
    /// <summary>Exports temporary draw DTOs for the three platform frames in native order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> DrawLists
    {
        get
        {
            foreach (ElevatorPlatformDraw platform in Enum.GetValues<ElevatorPlatformDraw>())
            {
                TryGetDraw(PlmDraw.Of(platform), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Maps a native platform draw pointer to its stable artwork identifier.</summary>
    internal static string VisualId(PlmDraw request) =>
        ElevatorPlatformPlmDefinitions.VisualIds.TryGetValue(
            PlmDraw.FrameOf<ElevatorPlatformDraw>(request, "elevator-platform draw list"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined elevator-platform draw list {request}.");

    /// <summary>Resolves a stable frame identifier to its temporary draw DTO.</summary>
    internal static bool TryGetByVisualId(string? id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(Enum.GetValues<ElevatorPlatformDraw>().Select(draw => (PlmDraw)PlmDraw.Of(draw)),
            VisualId, TryGetDraw, id, out list);

    /// <summary>Materializes an artwork DTO; gameplay reads platform cells directly.</summary>
    internal static bool TryGetDraw(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.TryCompile<Draw>(request, TryDescribe, out list);
}
