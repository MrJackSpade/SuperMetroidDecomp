namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The rocks Kraid's growth knocks from the ceiling, in the order they fall, one at a time; after the last no
/// more fall (#2400, #1473).</summary>
public static partial class KraidCeilingRockDefinitions
{
    /// <summary>
    /// The nine rocks. They alternate between the gaps at block columns 2 to 6 and 10 to 13, taking each gap's right
    /// then left remaining edge inward; each falls at the centre of the block it breaks on row 18, column 2 showing the
    /// left-edge background as it crumbles and the other columns their odd or even column's. Their irregular
    /// order is visible: the ceiling breaks from scattered points as his body rises, rather than sweeping across the room.
    /// </summary>
    public static IReadOnlyList<KraidCeilingRock> Rocks { get; } =
    [
        new(new(new(6, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
        new(new(new(13, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(new(new(2, 18), KraidRoomMutation.CrumbleCeilingAtLeftEdge)),
        new(new(new(10, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
        new(new(new(5, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(new(new(12, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
        new(new(new(3, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(new(new(11, 18), KraidRoomMutation.CrumbleCeilingInOddColumn)),
        new(new(new(4, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn)),
    ];
}
