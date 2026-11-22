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

    /// <summary>A room's enemies, which a check places Kraid in.</summary>
    extension(World.RoomEnemySystem enemies)
    {
        /// <summary>
        /// Places Kraid as his room's population places him (#2218, #2554): the room's load of his room's eight entries, his
        /// body first, whose entity begins his encounter, and each part after it, which joins it through the creation his body
        /// offers it, beside a lone Samus. The enemies load a room once, so a check places him in enemies of their own.
        /// </summary>
        /// <returns>The encounter his body began.</returns>
        internal KraidEnemyState PlaceKraid() => enemies.PlaceKraid(RoomEnemyFrameFixtures.StandaloneSamus());

        /// <summary>Places Kraid as his room's population places him as the parameterless placing does beside <paramref name="samus"/>.</summary>
        /// <returns>The encounter his body began.</returns>
        internal KraidEnemyState PlaceKraid(Samus.SamusState samus) =>
            enemies.LoadRoom(World.RoomStates.RoomEnemyPopulationDefinitions.Kraid, samus, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)))[0]
                .EntityOf<KraidBodyEntity>().Encounter;
    }
}
