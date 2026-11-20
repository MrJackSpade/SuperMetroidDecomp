using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// What a room state does while its room loads (#2287): a behaviour object, run once at each point of the load it acts at.
/// Once the room's effect is loaded and before the room composes its main, it composes its own parts into the room: the
/// scrolling sky's drifting bands, the Ceres haze or the Tourian statues' room (<see cref="Compose"/>, #2485). While the
/// room's objects are placed, once its own room objects and the door's setup are in and before the door closes behind
/// Samus and the room's enemies load, it may place a room object or hand Samus the area's map
/// (<see cref="PlaceRoomObjects"/>). Its earthquakes and the projectile one spawns belong to the room's enemies, so they
/// start once the enemies are loaded (<see cref="StartWithRoomEnemies"/>). <see cref="None"/> does nothing. Only the
/// import oracle relates each to the original game.
/// </summary>
public abstract class RoomSetupBehavior
{
    /// <summary>Names the behaviour.</summary>
    /// <param name="name">The behaviour's name, as the room states author it.</param>
    private protected RoomSetupBehavior(string name) => Name = Ensure.NotNull(name);

    /// <summary>The behaviour's name, as the room states author it.</summary>
    public string Name { get; }

    /// <summary>Composes the setup's own parts into the room being loaded, before the room composes its main.</summary>
    /// <param name="composition">The room being loaded and the owners the setup reaches as it composes its parts.</param>
    internal virtual void Compose(RoomSetupComposition composition) => Ensure.NotNull(composition);

    /// <summary>Runs the part of the setup that acts while the room's objects are placed.</summary>
    /// <param name="placing">The room and the owners the setup reaches while its objects are placed.</param>
    internal virtual void PlaceRoomObjects(RoomSetupPlacing placing) => Ensure.NotNull(placing);

    /// <summary>Runs the part of the setup that acts once the room's enemies are loaded.</summary>
    /// <param name="loaded">The room's enemies and the owners the setup reaches once they are loaded.</param>
    internal virtual void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded) => Ensure.NotNull(loaded);

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// No room-specific setup behaviour runs: the state names no setup, one that returns at once without changing
    /// anything (#1787), or Draygon's room's, which only restores the room's display effects when the game unpauses; the
    /// port draws the room without them, so nothing acts on it (#1827).
    /// </summary>
    public static RoomSetupBehavior None { get; } = new NoRoomSetup();

    /// <summary>Clears the rescued animals' exit blocks and starts shaking.</summary>
    public static RoomSetupBehavior OpenRescuedAnimalsPassageAndQuake { get; } = new MainstreetEscapeSetup();

    /// <summary>
    /// Places the fake wall of the old Tourian escape shaft, which destroys itself during the escape; the spawn silently
    /// drops it when every room-object place is taken.
    /// </summary>
    public static RoomSetupBehavior AutoDestroyWallDuringEscape { get; } = new OldTourianEscapeShaftWallSetup();

    /// <summary>
    /// Spawns the room object that turns the animals' rescue wall into shootable blocks (#2495); the spawn drops it, setup
    /// and all, when every room-object place is taken.
    /// </summary>
    public static RoomSetupBehavior TurnWallIntoShotBlocksDuringEscape { get; } = new EscapeAnimalWallSetup();

    /// <summary>Starts the landing site's escape quake and sets up the land scrolling sky with its bands.</summary>
    public static RoomSetupBehavior LandSkyWithLandingSiteQuake { get; } = new LandingSiteEscapeSetup();

    /// <summary>
    /// Spawns the scrolling sky's drifting bands, whichever door the room is entered through. The land and the ocean
    /// skies' setups are this one: which sky the bands drift over is the room's main's (#2485).
    /// </summary>
    public static RoomSetupBehavior SpawnScrollingSkyBands { get; } = new SkyBandsSetup();

    /// <summary>Starts the statue-unlocking animations during room setup.</summary>
    public static RoomSetupBehavior RunStatueUnlockingAnimations { get; } = new StatueUnlockingSetup();

    /// <summary>
    /// Holds an enemy-projectile place in the pre-Phantoon room for its first projectile updates
    /// (<see cref="PrePhantoonRoomHeldProjectilePlace"/>).
    /// </summary>
    public static RoomSetupBehavior HoldPrePhantoonProjectilePlace { get; } = new PrePhantoonProjectileSetup();

    /// <summary>Spawns Shaktool's room controller, which opens the room's first screen and closes the others.</summary>
    public static RoomSetupBehavior SpawnShaktoolController { get; } = new ShaktoolControllerSetup();

    /// <summary>Marks the room's area map as collected, as a map station would (#1799).</summary>
    public static RoomSetupBehavior SetCollectedMap { get; } = new CollectedMapSetup();

    /// <summary>Sets the timebomb event and starts light horizontal shaking.</summary>
    public static RoomSetupBehavior SetZebesTimebombEventAndLightHorizontalShaking { get; } = new TimebombEscapeSetup();

    /// <summary>Starts light horizontal escape-room shaking, clearing the diagonal quake the previous room's main left.</summary>
    public static RoomSetupBehavior SetLightHorizontalRoomShaking { get; } = new DiagonalClearingEscapeQuakeSetup(
        nameof(SetLightHorizontalRoomShaking), ZebesEscapeDefinitions.LightHorizontal);

    /// <summary>Starts medium horizontal escape-room shaking.</summary>
    public static RoomSetupBehavior SetMediumHorizontalRoomShaking { get; } = new EscapeQuakeSetup(
        nameof(SetMediumHorizontalRoomShaking), ZebesEscapeDefinitions.MediumHorizontal);

    /// <summary>
    /// Starts medium horizontal shaking in the escape room before the old Tourian escape shaft, clearing the diagonal quake
    /// the previous room's main left, and places the room object that starts its acid rising once Samus passes its trigger
    /// point (#1800).
    /// </summary>
    public static RoomSetupBehavior MediumHorizontalShakingWithRisingAcid { get; } = new RisingAcidEscapeSetup();

    /// <summary>
    /// Spawns the Ceres room haze during setup. Ceres Ridley's arena runs this setup too: its walls show the artwork its
    /// room loads for his fight, which the import pictures for the arena's states, and nothing the simulation does tells it
    /// from this one (#1791, #2485).
    /// </summary>
    public static RoomSetupBehavior SpawnCeresHaze { get; } = new CeresHazeSetup();

    /// <summary>
    /// The elevator shaft's setup once Ceres Ridley is dead (#2494): it spawns the Ceres haze, and places the room object
    /// that turns the blocks of the shaft's door solid, so Samus cannot walk back out through it during the escape.
    /// </summary>
    public static RoomSetupBehavior SealCeresElevatorDoorAndSpawnHaze { get; } = new CeresElevatorEscapeSetup();

    /// <summary>The setup that does nothing.</summary>
    private sealed class NoRoomSetup() : RoomSetupBehavior(nameof(None));

    /// <summary>A scrolling sky's setup, which spawns its bands.</summary>
    private sealed class SkyBandsSetup() : RoomSetupBehavior(nameof(SpawnScrollingSkyBands))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomSetupComposition composition) => Ensure.NotNull(composition).Scope.ComposeSkyBands();
    }

    /// <summary>
    /// A Ceres room's setup, which spawns its haze: red once Ceres Ridley is dead, and shown at once in a room loaded on its
    /// first view rather than through a door.
    /// </summary>
    private sealed class CeresHazeSetup() : RoomSetupBehavior(nameof(SpawnCeresHaze))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomSetupComposition composition) => ComposeCeresHaze(composition);
    }

    /// <summary>
    /// The elevator shaft's escape setup: the Ceres haze, and while the room's objects are placed, the seal of the shaft's
    /// door (#2494).
    /// </summary>
    private sealed class CeresElevatorEscapeSetup() : RoomSetupBehavior(nameof(SealCeresElevatorDoorAndSpawnHaze))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomSetupComposition composition) => ComposeCeresHaze(composition);

        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing) =>
            Ensure.NotNull(placing).RoomObjects.CeresEscape.SealElevatorDoor(placing.Level);
    }

    /// <summary>
    /// The statue room's setup, which starts the statues' unlocking: it composes their room, whose statues' enemies take it
    /// and whose sequence the room's load builds once its enemies are in (#2287, #2382).
    /// </summary>
    private sealed class StatueUnlockingSetup() : RoomSetupBehavior(nameof(RunStatueUnlockingAnimations))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomSetupComposition composition) =>
            Ensure.NotNull(composition).Scope.ComposeTourianStatueRoom(composition.FxOverlay);
    }

    /// <summary>An escape room's setup that starts its quake once the room's enemies are loaded.</summary>
    private sealed class EscapeQuakeSetup(string name, EarthquakeShake quake) : RoomSetupBehavior(name)
    {
        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded) =>
            Ensure.NotNull(loaded).Enemies.Earthquake.ShakeIndefinitely(quake);
    }

    /// <summary>
    /// An escape room's setup that, once the room's enemies are loaded, clears the diagonal quake the previous room's main
    /// left and starts its own quake.
    /// </summary>
    private sealed class DiagonalClearingEscapeQuakeSetup(string name, EarthquakeShake quake) : RoomSetupBehavior(name)
    {
        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded)
        {
            ClearDiagonalQuake(Ensure.NotNull(loaded));
            loaded.Enemies.Earthquake.ShakeIndefinitely(quake);
        }
    }

    /// <summary>The Landing Site's escape setup: its sky's bands, and its quake once the room's enemies are loaded.</summary>
    private sealed class LandingSiteEscapeSetup() : RoomSetupBehavior(nameof(LandSkyWithLandingSiteQuake))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomSetupComposition composition) => Ensure.NotNull(composition).Scope.ComposeSkyBands();

        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded) =>
            Ensure.NotNull(loaded).Enemies.Earthquake.ShakeIndefinitely(ZebesEscapeDefinitions.LandingQuake);
    }

    /// <summary>The first escape room's setup: once the room's enemies are loaded, its light quake, then the timebomb set.</summary>
    private sealed class TimebombEscapeSetup() : RoomSetupBehavior(nameof(SetZebesTimebombEventAndLightHorizontalShaking))
    {
        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded)
        {
            Ensure.NotNull(loaded).Enemies.Earthquake.ShakeIndefinitely(ZebesEscapeDefinitions.LightHorizontal);
            loaded.System.SetEvent(StoryEvent.ZebesTimebombSet);
        }
    }

    /// <summary>
    /// The escape room before the old Tourian escape shaft: it places the room object that raises its acid, then, once
    /// the room's enemies are loaded, clears the diagonal quake and starts its medium horizontal quake (#1800).
    /// </summary>
    private sealed class RisingAcidEscapeSetup() : RoomSetupBehavior(nameof(MediumHorizontalShakingWithRisingAcid))
    {
        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing) =>
            Ensure.NotNull(placing).RoomObjects.Tourian.SpawnEscapeAcidRise(placing.Level);

        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded)
        {
            ClearDiagonalQuake(Ensure.NotNull(loaded));
            loaded.Enemies.Earthquake.ShakeIndefinitely(ZebesEscapeDefinitions.MediumHorizontal);
        }
    }

    /// <summary>Mainstreet's escape setup: it places the rescued animals' passage, then starts the room's strong quake.</summary>
    private sealed class MainstreetEscapeSetup() : RoomSetupBehavior(nameof(OpenRescuedAnimalsPassageAndQuake))
    {
        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing) =>
            Ensure.NotNull(placing).RoomObjects.EscapeAnimals.SpawnCrateriaMainstreetEscapePassage(placing.Level);

        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded) =>
            Ensure.NotNull(loaded).Enemies.Earthquake.ShakeIndefinitely(ZebesEscapeDefinitions.MainstreetQuake);
    }

    /// <summary>The Climb's escape setup, which places the old Tourian escape shaft's fake wall.</summary>
    private sealed class OldTourianEscapeShaftWallSetup() : RoomSetupBehavior(nameof(AutoDestroyWallDuringEscape))
    {
        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing) =>
            Ensure.NotNull(placing).RoomObjects.Tourian.SpawnOldTourianEscapeShaftWall(placing.Level);
    }

    /// <summary>The rescued animals' room's escape setup, which spawns the room object that turns their wall into shootable blocks.</summary>
    private sealed class EscapeAnimalWallSetup() : RoomSetupBehavior(nameof(TurnWallIntoShotBlocksDuringEscape))
    {
        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing)
        {
            Ensure.NotNull(placing);
            placing.RoomObjects.EscapeAnimals.SpawnCrittersEscapeWall(placing.Level,
                placing.Level.GridBlock(new(EscapeAnimalWallDefinitions.WallX, EscapeAnimalWallDefinitions.WallY)));
        }
    }

    /// <summary>Shaktool's room's setup, which spawns Shaktool's room controller.</summary>
    private sealed class ShaktoolControllerSetup() : RoomSetupBehavior(nameof(SpawnShaktoolController))
    {
        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing) =>
            Ensure.NotNull(placing).RoomObjects.Shaktool.SpawnShaktoolRoomController(placing.Scrolls);
    }

    /// <summary>Tourian's first room's setup, which hands Samus its area's map as a map station would (#1799).</summary>
    private sealed class CollectedMapSetup() : RoomSetupBehavior(nameof(SetCollectedMap))
    {
        /// <inheritdoc/>
        internal override void PlaceRoomObjects(RoomSetupPlacing placing) =>
            Ensure.NotNull(placing).System.SetAreaMapAcquired(placing.Room.Area);
    }

    /// <summary>The pre-Phantoon room's setup, which holds its projectile place in the pool the enemies' load has just cleared.</summary>
    private sealed class PrePhantoonProjectileSetup() : RoomSetupBehavior(nameof(HoldPrePhantoonProjectilePlace))
    {
        /// <inheritdoc/>
        internal override void StartWithRoomEnemies(RoomSetupEnemiesLoaded loaded) =>
            Ensure.NotNull(loaded).Enemies.LoaderHooks.SpawnPrePhantoonRoomProjectile();
    }

    /// <summary>
    /// Composes the Ceres haze a Ceres room's setup spawns: red once Ceres Ridley is dead, and shown at once in a room loaded
    /// on its first view rather than through a door.
    /// </summary>
    private static void ComposeCeresHaze(RoomSetupComposition composition)
    {
        RoomScopeConstruction scope = Ensure.NotNull(composition).Scope;
        scope.ComposeCeresHaze(
            ridleyIsDead: composition.System.HasDefeatedAnyOf(scope.Room.Area, DefeatedBosses.AreaBoss),
            immediateViewport: composition.DisplaysInitialViewport);
    }

    /// <summary>Clears the diagonal quake the previous room's main left, as the two escape setups that switch quakes do.</summary>
    private static void ClearDiagonalQuake(RoomSetupEnemiesLoaded loaded) =>
        loaded.EscapeQuake.Clear();
}
