namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// One Shaktool chain (#2452): its seven segments and the motion and target they share. Its leading saw starts it when it
/// is placed, and every later segment joins it as it is placed; the chain lasts the room, and its death deletes all its
/// segments together.
/// </summary>
/// <remarks>
/// The game writes the chain's motion and target into every segment together; one owner holds them. A segment a hit kills
/// leaves its state emptied, which the rest of its chain still reaches through the update of the kill
/// (<see cref="RemovedEnemyLeftovers{TStates}"/>), but the emptied copy of the motion and the target never shows: a killed
/// segment runs no more, the chain-wide writes reach the emptied state as they reach the others, and the turn speeds the
/// chain sets after a wall read each segment's copy only after the motion and target are written to it.
/// </remarks>
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

    /// <summary>How the chain moves: its reversal, straightening and orbit direction.</summary>
    internal ShaktoolChainMotion Motion { get; set; }

    /// <summary>
    /// The heading the chain turns toward after a wall, a quarter turn on from the trailing saw's bearing from the leading
    /// saw; zero until the first wall. The trailing saw's reaching it starts the chain's curl again, the segments turn toward
    /// it at a share of the steps left (<see cref="ShaktoolOrbitDefinitions.ConvergenceShares"/>), and their head bob moves them along it.
    /// </summary>
    /// <remarks>
    /// The game also turns each segment's copy of the target a step on for every update the segment orbits while the chain
    /// straightens, the trailing saw's twice; no copy is read before the wall that ends the straightening writes the target
    /// again, so the chain's target does not turn.
    /// </remarks>
    internal RotationAngle TargetAngle { get; set; }
}
