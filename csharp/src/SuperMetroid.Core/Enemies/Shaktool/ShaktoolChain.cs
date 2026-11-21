namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// One Shaktool chain (#2452): its seven segments and the motion and target they share. Its leading saw starts it when it
/// is placed, and every later segment joins it as it is placed; the chain lasts the room, and its death deletes all its
/// segments together.
/// </summary>
internal sealed class ShaktoolChain
{
    /// <summary>Starts the chain of <paramref name="segments"/>, its leading saw first, moving as <paramref name="motion"/> says.</summary>
    /// <param name="segments">The chain's seven segments, leading saw first, each in its place with its definition.</param>
    /// <param name="motion">How the chain starts moving.</param>
    internal ShaktoolChain(IReadOnlyList<ShaktoolChainSegment> segments, ShaktoolChainMotion motion)
    {
        Segments = segments;
        Motion = motion;
    }

    /// <summary>The chain's seven segments, its leading saw first, each in its place with its definition.</summary>
    internal IReadOnlyList<ShaktoolChainSegment> Segments { get; }

    /// <summary>The chain's leading saw: its fixed anchor, the first enemy its room lists.</summary>
    internal RoomEnemyPlace LeadingSaw => Segments[0].Place;

    /// <summary>The place of the segment the chain lists right before the one <paramref name="segment"/> defines, which it orbits.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segment"/> is the chain's first, its leading saw, which orbits nothing.</exception>
    internal RoomEnemyPlace Before(ShaktoolSegmentDefinition segment) => Segments[Ensure.BetweenInclusive(1, Segments.Count - 1, IndexOf(segment)) - 1].Place;

    /// <summary>The segment the chain lists right after the one <paramref name="segment"/> defines, which the centre faces.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segment"/> is the chain's last, its trailing saw.</exception>
    internal ShaktoolChainSegment After(ShaktoolSegmentDefinition segment) => Segments[Ensure.BetweenInclusive(0, Segments.Count - 2, IndexOf(segment)) + 1];

    /// <summary>The state the chain holds for the segment <paramref name="segment"/> defines, which its setup initializes.</summary>
    internal ShaktoolSegmentState StateOf(ShaktoolSegmentDefinition segment) => Segments[IndexOf(segment)].State;

    /// <summary>The chain's segment <paramref name="segment"/> defines.</summary>
    internal ShaktoolChainSegment Segment(ShaktoolSegmentDefinition segment) => Segments[IndexOf(segment)];

    /// <summary>Where the chain lists the segment <paramref name="segment"/> defines.</summary>
    private int IndexOf(ShaktoolSegmentDefinition segment)
    {
        for (int listed = 0; listed < Segments.Count; listed++)
        {
            if (Segments[listed].Definition == segment)
                return listed;
        }
        throw new ArgumentOutOfRangeException(nameof(segment), segment, "The chain lists no such segment.");
    }

    /// <summary>How the chain moves: its reversal, straightening and orbit direction.</summary>
    internal ShaktoolChainMotion Motion { get; set; }

    /// <summary>
    /// The heading the chain turns toward after a wall, a quarter turn on from the trailing saw's bearing from the leading
    /// saw; zero until the first wall. The trailing saw's reaching it starts the chain's curl again, the segments turn toward
    /// it at a share of the steps left (<see cref="ShaktoolOrbitDefinitions.ConvergenceShares"/>), and their head bob moves them along it.
    /// </summary>
    internal RotationAngle TargetAngle { get; set; }
}
