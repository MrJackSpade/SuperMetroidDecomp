namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The rocks Kraid's growth knocks from the ceiling, in the order they fall (#2400).</summary>
/// <remarks>
/// The game drops the rocks one at a time from the first and stops after the last, so it only ever reads whole rocks: no
/// partial or past-the-end read is reachable, and none is reproduced (#1473).
/// </remarks>
public static class KraidCeilingRockDefinitions
{
    /// <summary>
    /// The nine rocks. They alternate between the gaps at block columns 2 to 6 and 10 to 13, taking each gap's right then
    /// left remaining edge inward; each falls at the centre of its block (16 times the column plus 8) on row 18, column 2
    /// showing the left-edge background as it crumbles and the other columns their odd or even column's. Their irregular
    /// order is visible: the ceiling breaks from scattered points as his body rises, rather than sweeping across the room.
    /// </summary>
    public static IReadOnlyList<KraidCeilingRock> Rocks { get; } =
    [
        new(104, new(new(6, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
        new(216, new(new(13, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(40, new(new(2, 18), KraidRoomMutation.CrumbleCeilingAtLeftEdge)),
        new(168, new(new(10, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
        new(88, new(new(5, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(200, new(new(12, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
        new(56, new(new(3, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(184, new(new(11, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(72, new(new(4, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
    ];
}
