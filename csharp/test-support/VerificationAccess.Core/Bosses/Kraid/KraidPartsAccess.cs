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

        /// <summary>
        /// Sets up the eight places the fixture has bound to Kraid's parts in the order his room lists them, as his room's
        /// population load sets them up (#2218, #2554): his body's entity begins his encounter, and each part joins it
        /// through the creation his body offers it, so his body's main AI runs as the enemy pass runs it.
        /// </summary>
        /// <returns>The encounter his body began.</returns>
        internal KraidEnemyState SetUpKraidInRoomOrder()
        {
            RoomEnemyPlace body = enemies.InUpdateOrder[0];
            body.SetUpAsPlaced();
            KraidEnemyState state = body.EntityOf<KraidBodyEntity>().Encounter;
            for (int place = 1; place < PartsInRoomOrder.Length; place++)
                enemies.InUpdateOrder[place].SetUpAsPlaced();
            return state;
        }
    }
}
