using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification set-up of Kraid's encounter for fixtures that place his state without his room's initialization.</summary>
internal static class KraidPartsAccess
{
    /// <summary>Kraid's eight parts in the order his room lists them.</summary>
    private static readonly EnemyDefinitionId[] PartsInRoomOrder =
    [
        EnemyDefinitionId.Kraid, EnemyDefinitionId.KraidArm, EnemyDefinitionId.KraidLintTop, EnemyDefinitionId.KraidLintMiddle,
        EnemyDefinitionId.KraidLintBottom, EnemyDefinitionId.KraidFoot, EnemyDefinitionId.KraidFirstNail, EnemyDefinitionId.KraidSecondNail,
    ];

    /// <summary>Fixture set-up of Kraid's parts.</summary>
    extension(EnemyPlaces enemies)
    {
        /// <summary>
        /// Binds the first eight places of <paramref name="enemies"/> to Kraid's parts in the order his room lists them, as
        /// his room loads them (#2558); a fixture binds them before it begins his encounter.
        /// </summary>
        internal void BindKraidPartsInRoomOrder()
        {
            for (int place = 0; place < PartsInRoomOrder.Length; place++)
            {
                RoomEnemyPlace partPlace = enemies.InUpdateOrder[place];
                partPlace.Bind(PartsInRoomOrder[place], partPlace.KeptDefinition);
            }
        }
    }

    /// <summary>Fixture set-up of Kraid's encounter.</summary>
    extension(KraidBehavior kraid)
    {
        /// <summary>
        /// Begins Kraid's encounter as his body's setup does, in the first place of <paramref name="enemies"/>, and sets up
        /// the enemies holding the next seven, which the fixture has bound to his parts in the order his room lists them,
        /// through the creations his body offers them (#2218, #2554), each joining the encounter as it is set up.
        /// </summary>
        /// <returns>The encounter begun.</returns>
        internal KraidEnemyState BeginEncounterInRoomOrder(EnemyPlaces enemies)
        {
            KraidEnemyState state = kraid.BeginKraid(enemies.InUpdateOrder[0]);
            for (int place = 1; place < PartsInRoomOrder.Length; place++)
                enemies.InUpdateOrder[place].SetUpAsPlaced();
            return state;
        }
    }
}
