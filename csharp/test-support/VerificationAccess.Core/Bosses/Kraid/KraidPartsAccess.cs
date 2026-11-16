using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification set-up of Kraid's encounter for fixtures that place his state without his room's initialization.</summary>
internal static class KraidPartsAccess
{
    /// <summary>Fixture set-up of Kraid's encounter.</summary>
    extension(KraidBehavior kraid)
    {
        /// <summary>
        /// Begins Kraid's encounter as his body's setup does and joins the first eight enemies of <paramref name="enemies"/>
        /// to it as his parts in the order his room lists them, each holding the encounter as its setup would (#1877, #2218).
        /// </summary>
        /// <returns>The encounter begun.</returns>
        internal KraidEnemyState BeginEncounterInRoomOrder(EnemyPlaces enemies)
        {
            KraidEnemyState state = kraid.Access.BeginEncounter();
            KraidParts parts = kraid.Parts;
            KraidPartReference[] roomOrder =
            [
                parts.BodyReference, parts.ArmReference, parts.TopLintReference, parts.MiddleLintReference,
                parts.BottomLintReference, parts.FootReference, parts.FirstNailReference, parts.SecondNailReference,
            ];
            for (int place = 0; place < roomOrder.Length; place++)
            {
                RoomEnemyPlace part = enemies.InUpdateOrder[place];
                roomOrder[place].Join(part);
                kraid.Access.AttachKraidPart(part, state);
            }
            return state;
        }
    }
}
