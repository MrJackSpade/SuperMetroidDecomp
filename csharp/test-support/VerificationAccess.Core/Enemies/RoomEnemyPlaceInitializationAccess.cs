using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
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
        internal void InitializeAsPlaced(SamusState samus) => self.InitializeAsPlaced(samus, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));

        /// <summary>
        /// Sets the slot's enemy up as the population load does with <paramref name="samus"/> in a room whose
        /// terrain is <paramref name="level"/>, for an enemy whose setup reads the room's terrain.
        /// </summary>
        internal void InitializeAsPlaced(SamusState samus, RoomLevelData level)
        {
            PopulationEnemySpawn spawn = EnemyPlaceRegistries.Of(self).Bind(self.HeldOccupant().Spawn);
            RoomEnemyOccupant occupant = self.Bind(spawn, self.KeptDefinition);
            SetUp(occupant, spawn, samus, level);
        }

        /// <summary>
        /// Sets the enemy the place holds up as the population load does once it has bound it and given it its entry's
        /// position, contact, processing and presence (#2554): its entry is bound to its species' creation through the
        /// place's registry, its species builds its entity in the place as it stands, and the entity runs its setup with
        /// <paramref name="samus"/> in a room whose terrain is <paramref name="level"/>.
        /// </summary>
        internal void SetUpAsLoaded(SamusState samus, RoomLevelData level)
        {
            RoomEnemyOccupant occupant = self.HeldOccupant();
            SetUp(occupant, EnemyPlaceRegistries.Of(self).Bind(occupant.Spawn), samus, level);
        }
    }

    /// <summary>
    /// Builds <paramref name="occupant"/>'s entity through <paramref name="spawn"/> and runs its setup, as the population
    /// load sets up an enemy it has placed.
    /// </summary>
    private static void SetUp(RoomEnemyOccupant occupant, PopulationEnemySpawn spawn, SamusState samus, RoomLevelData level)
    {
        var surroundings = new EnemyPopulationSurroundings(level, samus, GameplayAction.None, default);
        EnemyOccupant built = occupant.Place.Build(occupant, spawn.Loaded(surroundings));
        built.Initialization?.Initialize();
        built.SurroundingsInitialization?.Initialize(surroundings);
    }

    /// <summary>Verification placing of an enemy as a room load places it.</summary>
    extension(RoomEnemyPlace self)
    {

        /// <summary>
        /// Sets the slot's enemy up as the population load does, with a lone Samus (<see cref="InitializeAsPlaced"/>), so its
        /// species builds its entity and the state that entity holds.
        /// </summary>
        internal void SetUpAsPlaced() => self.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
    }
}
