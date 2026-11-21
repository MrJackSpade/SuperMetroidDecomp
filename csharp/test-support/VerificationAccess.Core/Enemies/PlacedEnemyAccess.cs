using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A check's enemy, set up and run as the game sets it up and runs it (#2554): its population entry is bound through the
/// place's registry and its species builds its entity as the population load does, and its main AI runs as the enemy pass
/// runs it, so a check drives the species through the entity the game builds rather than through its private steps.
/// </summary>
internal static class PlacedEnemyAccess
{
    /// <summary>The enemy a fixture place holds.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>
        /// Binds <paramref name="species"/> placed with <paramref name="settings"/> in the place and sets it up as the population
        /// load does with <paramref name="samus"/>, returning the entity its species built.
        /// </summary>
        /// <typeparam name="TEntity">The entity the species builds for the enemy.</typeparam>
        /// <exception cref="InvalidOperationException">The species built another entity.</exception>
        internal TEntity PlaceAs<TEntity>(EnemyDefinitionId species, EnemySpawnSettings settings, SamusState samus) where TEntity : class, IEnemyEntity
        {
            self.BindPlaced(species, settings);
            self.InitializeAsPlaced(samus);
            return self.EntityOf<TEntity>();
        }

        /// <summary>The entity the place's enemy is.</summary>
        /// <typeparam name="TEntity">The entity a check expects.</typeparam>
        /// <exception cref="InvalidOperationException">The place holds no such entity.</exception>
        internal TEntity EntityOf<TEntity>() where TEntity : class, IEnemyEntity =>
            self.Entity as TEntity
                ?? throw new InvalidOperationException($"Enemy place {self.Ordinal} holds {self.Entity?.GetType().Name ?? "no entity"}, not {typeof(TEntity).Name}.");

        /// <summary>Runs the place's enemy's main AI as the enemy pass runs it, with <paramref name="samus"/> on enemy frame <paramref name="enemyFrame"/>.</summary>
        internal void RunMainAi(SamusState samus, int enemyFrame = 0) => EnemyLifecycle.RunMainAi(self, AiFrame(samus, enemyFrame));

        /// <summary>Runs the place's enemy's main AI as the enemy pass runs it in <paramref name="frame"/>.</summary>
        internal void RunMainAi(IEnemyAiFrame frame) => EnemyLifecycle.RunMainAi(self, frame);
    }

    /// <summary>
    /// A main-AI frame over <paramref name="level"/>, an open room when none, with <paramref name="samus"/>, the camera at
    /// <paramref name="camera"/> or the room's origin, nothing pressed and both clocks at <paramref name="enemyFrame"/>.
    /// </summary>
    internal static IEnemyAiFrame AiFrame(SamusState samus, int enemyFrame = 0, RoomLevelData? level = null, Position16<RoomPixels> camera = default) =>
        new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(level ?? RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), enemyFrame) with { Samus = Ensure.NotNull(samus), Camera = camera },
            new FrameCounters(ShortFrameCount.Zero.After(enemyFrame)));

    /// <summary>A room of <paramref name="size"/> blocks that is solid throughout, so an enemy moving anywhere in it meets terrain.</summary>
    internal static RoomLevelData SolidRoom(Extent16<RoomBlocks> size)
    {
        int blockCount = checked(size.X * size.Y);
        var terrain = new RoomTerrainBlock[blockCount];
        Array.Fill(terrain, new RoomTerrainBlock(default, RoomCollisionType.SolidBlock));
        return new RoomLevelData(size, new RoomLevelPlanes(terrain, new RoomBlockBehavior[blockCount], new RoomBlockAppearance[blockCount]),
            RoomLevelData.NothingBeyondGrid(), []);
    }
}
