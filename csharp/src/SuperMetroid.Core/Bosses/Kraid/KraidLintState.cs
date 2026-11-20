using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's lints' own state beside what every part holds: the phase it runs, its X offset from his body and how
/// far it has travelled out of his mouth.
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidLintState : KraidAnimatedPartState
{
    /// <summary>Makes a lint of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the lint is part of.</param>
    /// <param name="place">The place the part occupies, which his body heads.</param>
    internal KraidLintState(KraidEnemyState kraid, RoomEnemyPlace place)
        : base(kraid, place, KraidPartSegment.LintInitial)
    {
    }

    /// <summary>
    /// Shows at once, for good, the pose the lint's animation beginning at <paramref name="segment"/> holds (#2400): the
    /// animation plays to that pose and rests there, and the lint's clock stops, so the shared animation never steps it
    /// again. A lint's setup shows its budding pose this way, and his growth its grown one.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation does not rest on a held pose first.</exception>
    internal void ShowPose(KraidPartSegment segment)
    {
        if (!ShowFirstPose(segment))
            throw new InvalidOperationException($"Kraid's lint animation {segment} holds no pose first.");
    }

    /// <summary>The phase the lint runs, inactive from its setup until his growth produces it.</summary>
    public KraidLintPhase Phase { get; internal set; }

    /// <summary>A lint's X offset from the body in pixels: zero, or sixteen to the left.</summary>
    public int LintXOffset { get; internal set; }

    /// <summary>The pixels a lint has travelled out of Kraid's mouth while it is produced; never negative.</summary>
    public int LintTravel
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}
