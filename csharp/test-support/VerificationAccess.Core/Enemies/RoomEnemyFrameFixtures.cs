using System.Runtime.CompilerServices;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy frames checks run enemy passes and enemy AI with (#1629). The game passes its own clocks, room, Samus's
/// projectiles and colours and the room's objects; a check that runs enemies outside the running game supplies them here.
/// </summary>
internal static class RoomEnemyFrameFixtures
{
    /// <summary>The inputs each enemy system's checks pass on every pass, made once per system as a room makes its own.</summary>
    private static readonly ConditionalWeakTable<RoomEnemySystem, StandaloneInputs> SystemInputs = [];

    /// <summary>
    /// A frame over <paramref name="level"/> with the camera at the room's origin, time running and a new Samus
    /// (<see cref="StandaloneSamus"/>): nothing held or newly pressed, both of the game's clocks at
    /// <paramref name="frameCounter"/>, idle new Samus projectile and bomb systems, Samus's starting colours, no turning
    /// floor and no camera checkpoint of Samus's previous position. A check sets what it needs with a <c>with</c> expression.
    /// </summary>
    internal static RoomEnemyFrame Standalone(RoomLevelData level, int frameCounter) =>
        Standalone(level, frameCounter, new StandaloneInputs());

    /// <summary>
    /// A load of <paramref name="population"/> into the room <paramref name="appearance"/> shows over, outside the running
    /// game (#2379): over <paramref name="level"/> beside <paramref name="samus"/>, nothing held, the camera at the room's
    /// origin, the ordinary gunship and the camera read as the load runs. A check sets what else it needs with a
    /// <c>with</c> expression.
    /// </summary>
    internal static RoomEnemyLoadRequest StandaloneLoad(RoomEnemyPopulationDefinition population, RoomAppearance appearance, RoomLevelData level, SamusState samus) =>
        new(population, appearance)
        {
            Level = Ensure.NotNull(level),
            Samus = Ensure.NotNull(samus),
            ControllerInput = GameplayAction.None,
            Camera = Position16<RoomPixels>.Zero,
            GunshipScenario = GunshipLoadScenario.Ordinary,
            DeferLoaderTimeCameraReads = false,
        };

    /// <summary>
    /// A room of <paramref name="size"/> blocks with no terrain in it, for a check whose enemies touch no terrain; the grid
    /// edge is solid, as a level that ends at its grid has it.
    /// </summary>
    internal static RoomLevelData OpenRoom(Extent16<RoomBlocks> size)
    {
        int blockCount = checked(size.X * size.Y);
        return new RoomLevelData(size,
            new RoomLevelPlanes(new RoomTerrainBlock[blockCount], new RoomBlockBehavior[blockCount], new RoomBlockAppearance[blockCount]),
            RoomLevelData.NothingBeyondGrid(), []);
    }

    /// <summary>
    /// A new Samus, as the game creates her, for a check whose enemies run beside her without the check placing her
    /// (#2010): every enemy update has Samus.
    /// </summary>
    internal static SamusState StandaloneSamus() => new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
        new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());

    /// <summary>
    /// Samus's held charge for a check whose touches run outside the enemy pass (#2159): idle new Samus projectiles and her
    /// starting colours, as the standalone frame holds them.
    /// </summary>
    internal static PseudoScrewCharge StandalonePseudoScrew() => new(InstalledFixtures.CreateProjectileSystem(), new SamusColours());

    /// <summary>
    /// The power bomb explosion of idle new Samus bombs, none exploding, for a check whose enemies run outside the enemy
    /// and projectile passes (#2159).
    /// </summary>
    internal static SamusPowerBombExplosionState StandalonePowerBombExplosion() => InstalledFixtures.CreateBombSystem().PowerBombExplosion;

    /// <summary>Builds the frame from one set of inputs.</summary>
    private static RoomEnemyFrame Standalone(RoomLevelData level, int frameCounter, StandaloneInputs inputs) => new()
    {
        Camera = Position16<RoomPixels>.Zero,
        TimeIsFrozen = false,
        Samus = inputs.Samus,
        NewlyPressedControllerInput = GameplayAction.None,
        ControllerInput = GameplayAction.None,
        SamusPreviousPositionCheckpoint = null,
        Level = Ensure.NotNull(level),
        SamusProjectiles = inputs.Projectiles,
        SamusColours = inputs.Colours,
        SamusBombs = inputs.Bombs,
        FrameCounter = frameCounter,
        ShortCounter = frameCounter % FrameClock.ShortCounterPeriod,
    };

    /// <summary>Supplies and runs a check's enemy passes on one enemy system.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>
        /// A frame for this system's next pass outside the running game, as <see cref="Standalone(RoomLevelData, int)"/>
        /// builds it, with both clocks at the system's random enemy counter, which advances once a pass, and the same
        /// projectile, bomb and colour inputs on every pass.
        /// </summary>
        internal RoomEnemyFrame StandaloneFrame(RoomLevelData level) =>
            Standalone(level, self.FrameContext.EnemyPassCount, SystemInputs.GetValue(self, _ => new StandaloneInputs()));

        /// <summary>
        /// Runs an enemy pass outside the running game: builds the processing list at the frame's camera first, as a door
        /// transition's enemy pass does, then runs the pass.
        /// </summary>
        internal EnemyFrameResult StepStandaloneFrame(RoomEnemyFrame frame)
        {
            Ensure.NotNull(frame);
            // A standalone frame is an update of its own: it begins the check's record of the requests the enemies join.
            self.Audio.Requests.BeginRecordedUpdate();
            self.PrepareEnemyProcessingList(frame.Camera);
            return self.StepFrame(frame);
        }

        /// <summary>
        /// Runs the ordinary shot pass outside the enemy pass (#2391): Samus's shots meet each interactive enemy in turn, in
        /// the order the enemy pass reaches them, with the camera at the room's origin.
        /// </summary>
        /// <returns>How many of the enemies a shot hit.</returns>
        internal int ResolveShotsOnInteractiveEnemies(SamusProjectileSystem projectiles, SamusBombProjectileSystem samusBombs, AreaId roomArea, SamusState samus)
        {
            int hits = 0;
            foreach (RoomEnemyPlace enemy in self.ProcessingWindow.InteractiveEnemies)
            {
                if (self.Collision.ResolveOrdinaryProjectileHit(projectiles, samusBombs, roomArea, samus, enemy, Position16<RoomPixels>.Zero))
                    hits++;
            }
            return hits;
        }

        /// <summary>
        /// Runs the ordinary bomb pass outside the enemy pass (#2391): Samus's bombs meet each interactive enemy in turn, in
        /// the order the enemy pass reaches them, with the camera at the room's origin.
        /// </summary>
        /// <returns>How many of the enemies a bomb hit.</returns>
        internal int ResolveBombsOnInteractiveEnemies(SamusBombProjectileSystem bombs, SamusProjectileSystem ordinaryProjectiles, SamusState samus)
        {
            int hits = 0;
            foreach (RoomEnemyPlace enemy in self.ProcessingWindow.InteractiveEnemies)
            {
                if (self.Collision.ResolveOrdinaryBombHit(bombs, ordinaryProjectiles, samus, enemy, Position16<RoomPixels>.Zero))
                    hits++;
            }
            return hits;
        }
    }

    /// <summary>A new Samus, idle Samus projectiles and bombs and Samus's starting colours.</summary>
    private sealed class StandaloneInputs
    {
        /// <summary>Samus, new.</summary>
        internal SamusState Samus { get; } = StandaloneSamus();

        /// <summary>Samus's beams and missiles, none fired.</summary>
        internal SamusProjectileSystem Projectiles { get; } = InstalledFixtures.CreateProjectileSystem();

        /// <summary>Samus's bombs, none laid.</summary>
        internal SamusBombProjectileSystem Bombs { get; } = InstalledFixtures.CreateBombSystem();

        /// <summary>Samus's colours in their starting shades.</summary>
        internal SamusColours Colours { get; } = new();
    }
}
