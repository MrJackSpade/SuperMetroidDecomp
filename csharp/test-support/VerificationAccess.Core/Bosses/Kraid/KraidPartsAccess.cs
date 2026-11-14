using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification set-up of <see cref="KraidParts"/> for fixtures that place Kraid's state without his room's initialization.</summary>
internal static class KraidPartsAccess
{
    /// <summary>The parts in the order Kraid's room lists them.</summary>
    private static readonly KraidPart[] RoomOrder =
    [
        KraidPart.Kraid, KraidPart.Arm, KraidPart.LintTop, KraidPart.LintMiddle, KraidPart.LintBottom, KraidPart.Foot,
        KraidPart.Nail, KraidPart.NailBad,
    ];

    /// <summary>Fixture joining of Kraid's parts.</summary>
    extension(KraidParts parts)
    {
        /// <summary>
        /// Joins the first eight enemies of <paramref name="enemies"/> to Kraid as his parts in the order his room lists
        /// them, as their initialization would (#1877).
        /// </summary>
        internal void JoinInRoomOrder(EnemySlotTable enemies)
        {
            for (int place = 0; place < RoomOrder.Length; place++)
                parts.Join(RoomOrder[place], enemies.Slots[place]);
        }
    }
}
