using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification set-up of <see cref="KraidParts"/> for fixtures that place Kraid's state without his room's initialization.</summary>
internal static class KraidPartsAccess
{
    /// <summary>Fixture joining of Kraid's parts.</summary>
    extension(KraidParts parts)
    {
        /// <summary>
        /// Joins the first eight enemies of <paramref name="enemies"/> to Kraid as his parts in the order his room lists
        /// them, as their initialization would (#1877).
        /// </summary>
        internal void JoinInRoomOrder(EnemyPlaces enemies)
        {
            KraidPartReference[] roomOrder =
            [
                parts.BodyReference, parts.ArmReference, parts.TopLintReference, parts.MiddleLintReference,
                parts.BottomLintReference, parts.FootReference, parts.FirstNailReference, parts.SecondNailReference,
            ];
            for (int place = 0; place < roomOrder.Length; place++)
                roomOrder[place].Join(enemies.InUpdateOrder[place]);
        }
    }
}
