using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification placing of an enemy as a room load places it.</summary>
internal static class RoomEnemyPlaceInitializationAccess
{
    /// <summary>Places the slot's enemy through its production initialization.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>
        /// Binds the slot's occupant again from the spawn settings recorded for it, as a room load records them before it
        /// binds the entity (#2342: a species whose placement names its part creates that part's entity), then runs the
        /// entity's initialization with <paramref name="samus"/> as the load hands her.
        /// </summary>
        internal void InitializeAsPlaced(SamusState samus) => self.InitializeAsPlaced(samus, []);

        /// <summary>
        /// Places the slot's occupant as the room load places it (<see cref="InitializeAsPlaced(SamusState)"/>); an occupant
        /// heading a group builds its parts in <paramref name="parts"/>, which already hold their identities, as the load
        /// reaches each after it (#2554).
        /// </summary>
        /// <param name="samus">Samus as the load hands her.</param>
        /// <param name="parts">The places the load lists right after the occupant, in order.</param>
        internal void InitializeAsPlaced(SamusState samus, IReadOnlyList<RoomEnemyPlace> parts)
        {
            var surroundings = new EnemyPopulationSurroundings(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), samus, GameplayAction.None, default);
            self.Bind(self.HeldOccupant().Spawn, self.KeptDefinition);
            IReadOnlyList<EnemyPartBuild> builds = EnemyLifecycle.SetUpEnemy(self, surroundings, parts);
            for (int part = 0; part < builds.Count; part++)
            {
                RoomEnemyPlace place = parts[part];
                place.Bind(place.DefinitionId, place.Definition);
                place.SetUpAsPart(builds[part], new EnemySetup.Loaded(surroundings));
            }
        }

        /// <summary>
        /// Sets the slot's occupant up through its species' factory as a room load places it, seeing a new Samus, and
        /// returns the <typeparamref name="T"/> its enemy holds (#2554); a group's head builds its parts in
        /// <paramref name="parts"/>.
        /// </summary>
        /// <param name="parts">The places the load lists right after the occupant, which already hold their identities.</param>
        /// <exception cref="InvalidOperationException">The occupant's enemy holds no <typeparamref name="T"/>.</exception>
        internal T SetUpPlaced<T>(params RoomEnemyPlace[] parts) where T : class
        {
            self.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus(), parts);
            return self.RequireSpeciesState<T>();
        }

        /// <summary>
        /// Binds the slot to <paramref name="definition"/>, with its compiled definition, and sets it up as a room load
        /// places it (<see cref="SetUpPlaced{T}"/>), returning the <typeparamref name="T"/> its enemy holds.
        /// </summary>
        /// <param name="definition">The occupant's identity.</param>
        /// <param name="parts">The places the load lists right after the occupant, which already hold their identities.</param>
        internal T SetUpPlacedAs<T>(EnemyDefinitionId definition, params RoomEnemyPlace[] parts) where T : class
        {
            self.Bind(definition, RoomEnemyDefinitionLookup.Get(definition));
            return self.SetUpPlaced<T>(parts);
        }
    }
}
