using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A check's enemy, run as the game runs it (#2554): a check places it through its room's population load
/// (<see cref="PopulationPlacementAccess"/>), and its main AI runs as the enemy pass runs it, so a check drives the species
/// through the entity the game builds rather than through its private steps.
/// </summary>
internal static class PlacedEnemyAccess
{
    /// <summary>The enemy a fixture place holds.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>The entity the place's enemy is.</summary>
        /// <typeparam name="TEntity">The entity a check expects.</typeparam>
        /// <exception cref="InvalidOperationException">The place holds no such entity.</exception>
        internal TEntity EntityOf<TEntity>() where TEntity : class, IEnemyEntity =>
            self.Entity as TEntity
                ?? throw new InvalidOperationException($"Enemy place {self.Ordinal} holds {self.Occupant?.DefinitionId.ToString() ?? "no enemy"}, whose entity is not the one the check expects.");

        /// <summary>Runs the place's enemy's main AI as the enemy pass runs it, with <paramref name="samus"/> on enemy frame <paramref name="enemyFrame"/>.</summary>
        internal void RunMainAi(SamusState samus, int enemyFrame = 0) => EnemyLifecycle.RunMainAi(self, AiFrame(samus, enemyFrame));

        /// <summary>Runs the place's enemy's main AI as the enemy pass runs it in <paramref name="frame"/>.</summary>
        internal void RunMainAi(IEnemyAiFrame frame) => EnemyLifecycle.RunMainAi(self, frame);

        /// <summary>
        /// Advances the place's enemy's animation as the enemy pass does after its main AI, with <paramref name="samus"/> in
        /// <paramref name="level"/>, an open room when none, nothing pressed and the camera at <paramref name="camera"/>.
        /// </summary>
        internal void AdvanceAnimation(SamusState samus, RoomLevelData? level = null, Position16<RoomPixels> camera = default) =>
            EnemyAnimationPlayer.AdvanceAnimation(self, Ensure.NotNull(samus), level ?? RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), camera,
                SuperMetroid.Core.Input.GameplayAction.None);

        /// <summary>Runs the place's enemy's main AI and then its animation, as one update of the enemy pass runs them.</summary>
        internal void RunUpdate(SamusState samus, int enemyFrame = 0, RoomLevelData? level = null, Position16<RoomPixels> camera = default)
        {
            EnemyLifecycle.RunMainAi(self, AiFrame(samus, enemyFrame, level, camera));
            self.AdvanceAnimation(samus, level, camera);
        }
    }

    /// <summary>
    /// A main-AI frame over <paramref name="level"/>, an open room when none, with <paramref name="samus"/>, the camera at
    /// <paramref name="camera"/> or the room's origin, nothing pressed and both clocks at <paramref name="enemyFrame"/>.
    /// </summary>
    internal static IEnemyAiFrame AiFrame(SamusState samus, int enemyFrame = 0, RoomLevelData? level = null, Position16<RoomPixels> camera = default) =>
        new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(level ?? RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), enemyFrame) with { Samus = Ensure.NotNull(samus), Camera = camera },
            new FrameCounters(ShortFrameCount.Zero.After(enemyFrame)));

    /// <summary>
    /// A main-AI frame over an open room with <paramref name="samus"/>, the camera at <paramref name="camera"/>, Samus's
    /// bombs <paramref name="bombs"/>, nothing pressed and both clocks at zero: the frame an enemy that reads the room's
    /// bombs, such as the Baby Metroid draining Samus, runs in beside the running game's Samus.
    /// </summary>
    internal static IEnemyAiFrame AiFrame(SamusState samus, Position16<RoomPixels> camera, SamusBombProjectileSystem bombs) =>
        new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with
            {
                Samus = Ensure.NotNull(samus), Camera = camera, SamusBombs = Ensure.NotNull(bombs),
            },
            new FrameCounters(ShortFrameCount.Zero));

    /// <summary>
    /// A main-AI frame over an open room with <paramref name="samus"/>, the camera at the room's origin,
    /// <paramref name="newlyPressed"/> newly pressed, Samus's beams and missiles <paramref name="projectiles"/> and both
    /// clocks at zero: the frame an enemy that answers the controller, such as an elevator, runs in.
    /// </summary>
    internal static IEnemyAiFrame AiFrame(SamusState samus, GameplayAction newlyPressed, SamusProjectileSystem projectiles) =>
        new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with
            {
                Samus = Ensure.NotNull(samus), NewlyPressedControllerInput = newlyPressed, SamusProjectiles = Ensure.NotNull(projectiles),
            },
            new FrameCounters(ShortFrameCount.Zero));

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
