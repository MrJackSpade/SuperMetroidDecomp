using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A check's enemies placed as the game places them (#2554): the room's population load binds each entry to its species
/// through the room's own registry, initializes its place from its definition and entry, and runs its setup, so a check
/// drives the species through the entity the game builds. The enemies load a room once, so a check places all the enemies
/// it needs at once, in enemies of their own.
/// </summary>
internal static class PopulationPlacementAccess
{
    /// <summary>
    /// An entry placing <paramref name="species"/> at <paramref name="at"/> with <paramref name="settings"/> and no contact,
    /// processing or presence of its own, as a check authors one.
    /// </summary>
    internal static RoomEnemyPlacement Entry(EnemyDefinitionId species, EnemySpawnSettings settings, Position16<RoomPixels> at = default) =>
        new(species, EnemyPlacing.At(at), EnemyContact.None, EnemyProcessing.None, EnemyPresence.None, CollidesByFrameHitboxes: false, settings);

    /// <summary>An entry placing <paramref name="species"/> at <paramref name="at"/> with no settings, as a check authors one.</summary>
    internal static RoomEnemyPlacement Entry(EnemyDefinitionId species, Position16<RoomPixels> at = default) =>
        Entry(species, EnemySpawnSettings.None, at);

    /// <summary>A room's enemies, which a check places its population in.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Loads <paramref name="entries"/> as the room's population, beside a lone Samus in an open room, and returns the
        /// room's places in their order, the first holding the first entry's enemy.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadPopulation(params RoomEnemyPlacement[] entries) =>
            enemies.LoadPopulation(RoomEnemyFrameFixtures.StandaloneSamus(), entries);

        /// <summary>
        /// Loads <paramref name="entries"/> as the room's population beside <paramref name="samus"/> in an open room, and
        /// returns the room's places in their order.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadPopulation(SamusState samus, params RoomEnemyPlacement[] entries) =>
            enemies.LoadPopulation(samus, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), entries);

        /// <summary>
        /// Loads <paramref name="entries"/> as the room's population beside <paramref name="samus"/> in a room whose terrain
        /// is <paramref name="level"/>, for enemies whose setup reads the room's terrain, and returns the room's places in
        /// their order. The population draws under the session start's graphics set, as a room that has placed no enemies
        /// draws.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadPopulation(SamusState samus, RoomLevelData level, params RoomEnemyPlacement[] entries) =>
            enemies.LoadPopulation(samus, level, GameplayAction.None, entries);

        /// <summary>
        /// Loads <paramref name="entries"/> as the three-argument load does while the controller holds
        /// <paramref name="controllerInput"/>, for an enemy whose setup reads the buttons held as its room loads.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadPopulation(SamusState samus, RoomLevelData level, GameplayAction controllerInput,
            params RoomEnemyPlacement[] entries)
        {
            var population = new PlacedRoomEnemyPopulationDefinition("fixture population", Ensure.NotNull(entries), deathQuota: 0);
            FixturePopulationSets.Record(population, RoomEnemyGraphicsSetDefinitions.SessionStart);
            return enemies.LoadRoom(population, samus, level, controllerInput);
        }

        /// <summary>
        /// Loads, as the room's population, the entries a check authored on the room's first places with the place
        /// helpers that write an entry's settings or words (<see cref="RoomEnemyPlaceSpawnParameters"/>), in their order up
        /// to the first place it authored none on, beside a lone Samus in an open room. Each entry places its enemy where the
        /// check stood its place, when it places it at a position, and gives it the contact, processing and presence the
        /// check gave the place, as the place's entry would, and its species the definition the check bound on the place
        /// (<see cref="AuthoredEnemyDefinitions"/>). Returns the room's places.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadAuthoredPopulation() =>
            enemies.LoadAuthoredPopulation(RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));

        /// <summary>
        /// Loads the entries a check authored on the room's first places as the parameterless load does beside
        /// <paramref name="samus"/> in a room whose terrain is <paramref name="level"/>.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadAuthoredPopulation(SamusState samus, RoomLevelData level) =>
            enemies.LoadAuthoredPopulation(samus, level, GameplayAction.None);

        /// <summary>
        /// Loads the entries a check authored on the room's first places as the two-argument load does while the controller
        /// holds <paramref name="controllerInput"/>, for an enemy whose setup reads the buttons held as its room loads.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadAuthoredPopulation(SamusState samus, RoomLevelData level, GameplayAction controllerInput)
        {
            var entries = new List<RoomEnemyPlacement>();
            AuthoredEnemyDefinitions definitions = AuthoredEnemyDefinitions.Of(Ensure.NotNull(enemies));
            foreach (RoomEnemyPlace place in enemies.Places.InUpdateOrder)
            {
                if (place.History is not PopulatedEnemyPlace populated)
                    break;
                // The load builds each enemy with the definition the check bound on its place.
                definitions.Author(place.HeldOccupant().DefinitionId, place.HeldOccupant().Definition);
                RoomEnemyPlacement authored = populated.Occupancy.LastSpawn;
                entries.Add((authored.Placing is EnemyPlacedAt ? authored with { Placing = EnemyPlacing.At(place.Position.Whole) } : authored)
                    with { Contact = place.Contact, Processing = place.Processing, Presence = place.Presence });
            }
            // The load meets each place holding no enemy, as a room's load meets its places: an earlier enemy's setup that
            // reads a later place finds it empty, not holding the entry the check authored there.
            foreach (RoomEnemyPlace place in enemies.Places.InUpdateOrder.Take(entries.Count))
                place.Unbind();
            return enemies.LoadPopulation(samus, level, controllerInput, [.. entries]);
        }

        /// <summary>
        /// Loads the first <paramref name="count"/> entries of retail <paramref name="room"/>'s population, which keep its name
        /// and so draw from its artwork, beside <paramref name="samus"/> in an open room, and returns the room's places.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadFirstEntries(RoomEnemyPopulationDefinition room, int count, SamusState samus)
        {
            RoomEnemyPlacement[] entries = Ensure.NotNull(room).Placements.Span[..count].ToArray();
            return enemies.LoadRoom(new PlacedRoomEnemyPopulationDefinition(room.Name, entries, deathQuota: 0), samus, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
        }

        /// <summary>Loads <paramref name="population"/> beside <paramref name="samus"/> in <paramref name="level"/> and returns the room's places.</summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadRoom(RoomEnemyPopulationDefinition population, SamusState samus, RoomLevelData level) =>
            enemies.LoadRoom(population, samus, level, GameplayAction.None);

        /// <summary>
        /// Loads <paramref name="population"/> as the three-argument load does while the controller holds
        /// <paramref name="controllerInput"/>.
        /// </summary>
        internal IReadOnlyList<RoomEnemyPlace> LoadRoom(RoomEnemyPopulationDefinition population, SamusState samus, RoomLevelData level,
            GameplayAction controllerInput)
        {
            Ensure.NotNull(enemies).Load(RoomEnemyFrameFixtures.StandaloneLoad(population,
                BgArtworkFixtures.For(new SuperMetroid.AssetExtraction.Video.SnesVram()), level, Ensure.NotNull(samus)) with { ControllerInput = controllerInput });
            return enemies.Places.InUpdateOrder;
        }

        /// <summary>
        /// Loads <paramref name="species"/> placed with <paramref name="settings"/> at <paramref name="at"/> as the room's
        /// only enemy, beside <paramref name="samus"/> in <paramref name="level"/> or an open room, and returns the entity its
        /// species built.
        /// </summary>
        /// <typeparam name="TEntity">The entity the species builds for the enemy.</typeparam>
        /// <exception cref="InvalidOperationException">The species built another entity.</exception>
        internal TEntity PlaceAs<TEntity>(EnemyDefinitionId species, EnemySpawnSettings settings, SamusState samus,
            Position16<RoomPixels> at = default, RoomLevelData? level = null) where TEntity : class, IEnemyEntity =>
            enemies.LoadPopulation(samus, level ?? RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Entry(species, settings, at))[0].EntityOf<TEntity>();
    }
}
