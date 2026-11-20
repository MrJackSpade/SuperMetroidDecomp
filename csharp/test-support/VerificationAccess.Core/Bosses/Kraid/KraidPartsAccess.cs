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
        /// Begins Kraid's encounter as his body's setup does and joins the enemies holding the first eight places of
        /// <paramref name="enemies"/>, which the fixture has bound to his parts, to it as his parts in the order his room
        /// lists them, each holding the encounter as its setup would (#1877, #2218), and shows it on his body layer as his
        /// body's setup does (#2194).
        /// </summary>
        /// <returns>The encounter begun.</returns>
        internal KraidEnemyState BeginEncounterInRoomOrder(EnemyPlaces enemies)
        {
            KraidParts parts = kraid.Parts;
            var state = new KraidEnemyState(parts);
            kraid.BodyLayer.Show(state);
            (KraidPartReference Reference, KraidPartState State)[] roomOrder =
            [
                (parts.BodyReference, state.BodyState), (parts.ArmReference, state.ArmState), (parts.TopLintReference, state.TopLintState),
                (parts.MiddleLintReference, state.MiddleLintState), (parts.BottomLintReference, state.BottomLintState),
                (parts.FootReference, state.FootState), (parts.FirstNailReference, state.FirstNailState), (parts.SecondNailReference, state.SecondNailState),
            ];
            for (int place = 0; place < roomOrder.Length; place++)
            {
                roomOrder[place].Reference.Join(enemies.InUpdateOrder[place].HeldOccupant());
                KraidEncounterAccess.AttachKraidPart(roomOrder[place].State);
            }
            return state;
        }
    }
}
