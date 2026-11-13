using System.Runtime.CompilerServices;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Palettes;
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
    /// A frame over <paramref name="level"/> with the camera at the room's origin, time running and no Samus: both of the
    /// game's clocks at <paramref name="frameCounter"/>, idle new Samus projectile and bomb systems, Samus's starting
    /// colours and the breakable terrain of an empty room-object table. A check sets what it needs with a <c>with</c>
    /// expression.
    /// </summary>
    internal static RoomEnemyFrame Standalone(RoomLevelData level, int frameCounter) =>
        Standalone(level, frameCounter, new StandaloneInputs());

    /// <summary>
    /// A room of <paramref name="size"/> blocks with no terrain in it, for a check whose enemies touch no terrain; the grid
    /// edge is solid, as a level that ends at its grid has it.
    /// </summary>
    internal static RoomLevelData OpenRoom(Extent16<RoomBlocks> size)
    {
        int blockCount = checked(size.X * size.Y);
        return new RoomLevelData(size,
            new RoomLevelPlanes(new RoomTerrainBlock[blockCount], new RoomBlockBehavior[blockCount], new RoomBlockAppearance[blockCount]),
            RoomLevelData.NothingBeyondGrid());
    }

    /// <summary>Builds the frame from one set of inputs.</summary>
    private static RoomEnemyFrame Standalone(RoomLevelData level, int frameCounter, StandaloneInputs inputs) => new()
    {
        Camera = Position16<RoomPixels>.Zero,
        TimeIsFrozen = false,
        Samus = null,
        Level = Ensure.NotNull(level),
        SamusProjectiles = inputs.Projectiles,
        SamusColours = inputs.Colours,
        SharedProjectiles = inputs.Bombs,
        CollisionObjects = inputs.CollisionObjects,
        FrameCounter = frameCounter,
        ShortCounter = frameCounter % FrameClock.ShortCounterPeriod,
    };

    /// <summary>Supplies and runs a check's enemy passes on one enemy system.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>
        /// A frame for this system's next pass outside the running game, as <see cref="Standalone(RoomLevelData, int)"/>
        /// builds it, with both clocks at the system's random enemy counter, which advances once a pass, and the same
        /// projectile, bomb, colour and room-object inputs on every pass.
        /// </summary>
        internal RoomEnemyFrame StandaloneFrame(RoomLevelData level) =>
            Standalone(level, self.FrameContext.RandomEnemyCounter, SystemInputs.GetValue(self, _ => new StandaloneInputs()));

        /// <summary>
        /// Runs an enemy pass outside the running game: builds the processing list at the frame's camera first, as a door
        /// transition's enemy pass does, then runs the pass.
        /// </summary>
        internal EnemyFrameResult StepStandaloneFrame(RoomEnemyFrame frame)
        {
            Ensure.NotNull(frame);
            // A standalone frame is an update of its own: it begins the record the enemies' requests join, as the runtime's
            // update begins its own.
            self.Audio.Requests.BeginUpdate();
            self.ProcessingWindow.PrepareEnemyProcessingList(frame.Camera);
            return self.StepFrame(frame);
        }
    }

    /// <summary>Idle Samus projectiles and bombs, Samus's starting colours and an empty room-object table's breakable terrain.</summary>
    private sealed class StandaloneInputs
    {
        /// <summary>Samus's beams and missiles, none fired.</summary>
        internal SamusProjectileSystem Projectiles { get; } = InstalledFixtures.CreateProjectileSystem();

        /// <summary>Samus's bombs, none laid.</summary>
        internal SamusBombProjectileSystem Bombs { get; } = InstalledFixtures.CreateBombSystem();

        /// <summary>Samus's colours in their starting shades.</summary>
        internal SamusColours Colours { get; } = new();

        /// <summary>The enemy-breakable terrain of a room-object table with no objects.</summary>
        internal EnemyBreakableTerrainObjects CollisionObjects { get; } = new(new RoomObjectSlotTable(new RoomObjectPlaces()));
    }
}
