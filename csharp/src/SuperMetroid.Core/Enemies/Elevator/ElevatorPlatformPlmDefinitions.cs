using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The $B70B elevator-platform PLM's complete $84:AFB6 instruction loop and its
/// three tile-layout lists. Timing, list selection, collision words, and offsets
/// are cartridge-defined; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformPlmDefinitions
{

    /// <summary>
    /// Three frames at $84:AA97..AADE, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// Continuations are absolute offsets (3,0), (0,1), then the terminator.
    /// </summary>
    internal readonly record struct Draw(int Frame) : IPlmRunDrawShape
    {
        /// <summary>Gets the three native strips.</summary>
        public int RunCount => 3;
        /// <summary>Rejects a strip index outside this platform draw.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Gets one edge cell or four lower-row cells for a selected strip.</summary>
        public int WordCount(int run)
        {
            CheckRun(run);
            return run == 2 ? 4 : 1;
        }
        /// <summary>Gets the next strip's absolute horizontal offset from the PLM origin.</summary>
        public sbyte NextX(int run)
        {
            CheckRun(run);
            return run == 0 ? (sbyte)3 : (sbyte)0;
        }
        /// <summary>Gets the next strip's absolute vertical offset from the PLM origin.</summary>
        public sbyte NextY(int run)
        {
            CheckRun(run);
            return run == 1 ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>Calculates a solid platform tile word, mirroring right-side artwork where needed.</summary>
        public ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            bool right = run == 1 || run == 2 && cell >= 2;
            int tile = run < 2 ? 0x85 + Frame : 0x88 + Frame * 2 + Math.Min(cell, 3 - cell);
            return (ushort)(0x8000 | (right ? 0x400 : 0) | tile);
        }
    }
    /// <summary>Recognizes one of the three platform frames and derives its frame index.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        // Draws of other families are not the platform's.
        draw = request is PlmFrameDraw<ElevatorPlatformDraw> { Frame: var platform }
            ? new(platform switch
            {
                ElevatorPlatformDraw.First => 0,
                ElevatorPlatformDraw.Second => 1,
                ElevatorPlatformDraw.Third => 2,
                _ => throw new InvalidOperationException($"Undefined {nameof(ElevatorPlatformDraw)} {platform}."),
            })
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
        PlmDraw.FrameOf<ElevatorPlatformDraw>(request, "elevator-platform draw list") switch
        {
            ElevatorPlatformDraw.First => "first-frame",
            ElevatorPlatformDraw.Second => "second-frame",
            ElevatorPlatformDraw.Third => "third-frame",
            _ => throw new InvalidOperationException($"Undefined elevator-platform draw list {request}."),
        };

    /// <summary>Resolves a stable frame identifier to its temporary draw DTO.</summary>
    internal static bool TryGetByVisualId(string? id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Enum.GetValues<ElevatorPlatformDraw>().Select(draw => (PlmDraw)PlmDraw.Of(draw)),
            VisualId, TryGetDraw, id, out list);

    /// <summary>Materializes an artwork DTO; gameplay calculates platform cells directly.</summary>
    internal static bool TryGetDraw(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompile<Draw>(request, TryDescribe, out list);
}
