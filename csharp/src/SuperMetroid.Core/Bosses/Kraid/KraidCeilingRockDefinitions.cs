namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The rocks Kraid's growth knocks from the ceiling, in the order they fall, one at a time; after the last no
/// more fall (#2400, #1473).</summary>
public static class KraidCeilingRockDefinitions
{
    /// <summary>
    /// The nine rocks. They alternate between the gaps at block columns 2 to 6 and 10 to 13, taking each gap's right
    /// then left remaining edge inward; each falls at the centre of the block it breaks on row 18, column 2 showing the
    /// left-edge background as it crumbles and the other columns their odd or even column's. Their irregular
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
