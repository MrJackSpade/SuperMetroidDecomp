namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a projectile trail starts when its read passes its placement list (#1318, #1411, #1474). A trail places its two
/// sparkles from the placement its shot's animation frame selects in the trail's list. A shot whose list has only just
/// started has shown no frame yet, and the original game reads the word stored before the list as its frame; and a frame
/// past the list's run of placements reads whatever follows the run. Both reads are decoded once by the import pipeline
/// into the frame each shot reads and the placement each list yields at each such frame; a read that reaches scratch
/// memory draws no trail (owner decision on #1318).
/// </summary>
/// <remarks>
/// Consulted when a shot spawns a trail, before the trail slot is allocated. Evidence: the trail coordinate check, which
/// compares every placement each list yields past its run, and every frame a fresh list reads, with the original game's
/// tables and reads.
/// </remarks>
/// <typeparam name="TProgram">A shot's animation program.</typeparam>
/// <typeparam name="TList">A trail placement list.</typeparam>
/// <typeparam name="TPlacement">Where a trail's sparkles start.</typeparam>
[CompatibilityService("a projectile trail whose start read passes its placement list starts from what lies past it")]
public sealed class TrailStartPastPlacements<TProgram, TList, TPlacement>
    where TProgram : notnull
    where TList : notnull
    where TPlacement : struct
{
    /// <summary>The animation frame a trail reads from a shot whose list has just started, by the shot's program.</summary>
    private readonly IReadOnlyDictionary<TProgram, int> _framesAtListStart;

    /// <summary>What each list yields at each frame a trail can read past its run; null where the read reaches scratch.</summary>
    private readonly IReadOnlyDictionary<TList, IReadOnlyDictionary<int, TPlacement?>> _pastRecords;

    /// <summary>Keeps the decoded frames and placements.</summary>
    /// <param name="framesAtListStart">The frame a trail reads from a shot whose list has just started, by program, 0 through 65535.</param>
    /// <param name="pastRecords">What each list yields at each frame a trail can read past its run.</param>
    public TrailStartPastPlacements(
        IReadOnlyDictionary<TProgram, int> framesAtListStart,
        IReadOnlyDictionary<TList, IReadOnlyDictionary<int, TPlacement?>> pastRecords)
    {
        _framesAtListStart = Ensure.NotNull(framesAtListStart);
        _pastRecords = Ensure.NotNull(pastRecords);
    }

    /// <summary>The animation frame a trail reads from a shot of <paramref name="program"/> whose list has just started.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The installation decoded no frame for the program.</exception>
    public int FrameAtListStart(TProgram program) =>
        _framesAtListStart.TryGetValue(program, out int frame)
            ? frame
            : throw new ArgumentOutOfRangeException(nameof(program), program, "No frame was decoded for a trail read at this program's start.");

    /// <summary>
    /// What a trail reads from <paramref name="list"/> at animation frame <paramref name="frame"/>, past the list's run of
    /// placements; null where the read reaches scratch memory, which draws no trail.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No trail reads <paramref name="list"/> past its run at <paramref name="frame"/>.</exception>
    public TPlacement? PastRecords(TList list, int frame) =>
        _pastRecords.TryGetValue(list, out IReadOnlyDictionary<int, TPlacement?>? frames) &&
        frames.TryGetValue(frame, out TPlacement? placement)
            ? placement
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, $"No trail reads {list} past its placements at that frame.");
}
