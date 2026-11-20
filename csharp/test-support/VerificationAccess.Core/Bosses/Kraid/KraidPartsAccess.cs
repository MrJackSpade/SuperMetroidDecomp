using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's parts for checks that build his encounter themselves (#2554).</summary>
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

    /// <summary>The parts of a Kraid whose body and seven parts a fixture room lists first, in room order.</summary>
    /// <param name="enemies">The fixture room's enemy places.</param>
    internal static KraidParts InRoomOrder(EnemyPlaces enemies) =>
        new(enemies.InUpdateOrder[0], [.. enemies.InUpdateOrder.Skip(1).Take(KraidParts.PartCount)]);

    /// <summary>The parts of a Kraid standing in eight places of their own, for a check that reads no room.</summary>
    internal static KraidParts Standalone()
    {
        RoomEnemyPlace[] places = [.. Enumerable.Range(0, KraidParts.PartCount + 1)
            .Select(ordinal => new RoomEnemyPlace(ordinal, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers()))];
        return new KraidParts(places[0], places[1..]);
    }

    extension(KraidBehavior kraid)
    {
        /// <summary>Begins an encounter over the fixture room's first eight places, which the body layer shows.</summary>
        /// <param name="enemies">The fixture room's enemy places.</param>
        internal KraidEnemyState BeginEncounterInRoomOrder(EnemyPlaces enemies)
        {
            var state = new KraidEnemyState(InRoomOrder(enemies));
            kraid.BodyLayer.Show(state);
            return state;
        }
    }
}
