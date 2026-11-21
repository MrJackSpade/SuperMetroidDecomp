using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification placing of an enemy as a room load places it.</summary>
internal static class RoomEnemyPlaceInitializationAccess
{
    /// <summary>Places the slot's enemy through its production setup.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>
        /// Binds the slot's occupant again from the placement recorded for it through the place's registry, as a room load
        /// binds it (#2342: a species whose placement names its part creates that part's entity), then sets it up as the
        /// population load does (#2554): its species builds its entity, which runs its setup with <paramref name="samus"/> as
        /// the load hands her.
        /// </summary>
        internal void InitializeAsPlaced(SamusState samus)
        {
            PopulationEnemySpawn spawn = EnemyPlaceRegistries.Of(self).Bind(self.HeldOccupant().Spawn);
            RoomEnemyOccupant occupant = self.Bind(spawn, self.KeptDefinition);
            var surroundings = new EnemyPopulationSurroundings(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), samus, GameplayAction.None, default);
            EnemyOccupant built = self.Build(occupant, spawn.Loaded(surroundings));
            built.Initialization?.Initialize();
            built.SurroundingsInitialization?.Initialize(surroundings);
        }

        /// <summary>
        /// Sets the slot's enemy up as the population load does, with a lone Samus (<see cref="InitializeAsPlaced"/>), so its
        /// species builds its entity and the state that entity holds.
        /// </summary>
        internal void SetUpAsPlaced() => self.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
    }
}
