using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// What a room state runs on every gameplay update with the room's own update, after gameplay drawing and the HUD and
/// before the room shakes (#2287). Each is a behaviour object: a room's load composes the main its state names into the
/// room, with the state that main keeps, and the room runs it once an update; <see cref="None"/> composes nothing. Only
/// the import oracle relates each to the original game.
/// </summary>
public abstract class RoomMainBehavior
{
    /// <summary>Names the behaviour.</summary>
    /// <param name="name">The behaviour's name, as the room states author it.</param>
    private protected RoomMainBehavior(string name) => Name = Ensure.NotNull(name);

    /// <summary>The behaviour's name, as the room states author it.</summary>
    public string Name { get; }

    /// <summary>The sky the main scrolls behind the room, or null for a main that scrolls none.</summary>
    public virtual ScrollingSky? Sky => null;

    /// <summary>Whether the main sets off the escape's random explosions, which the import pictures only for its rooms.</summary>
    public virtual bool SetsOffEscapeExplosions => false;

    /// <summary>Has the room <paramref name="scope"/> builds compose this main, with the state it keeps.</summary>
    /// <param name="scope">The room being loaded: its door, its state and the working value the previous room's main left.</param>
    internal abstract void Compose(RoomScopeConstruction scope);

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// No room-specific main behaviour runs: the state names no main, one that returns at once and leaves the working
    /// value <see cref="RoomMainCarryover"/> holds untouched (#1787), or one whose only work the port does not reproduce:
    /// the Dachora room's and Ridley's room's (#2287).
    /// </summary>
    public static RoomMainBehavior None { get; } = new NoRoomMain();

    /// <summary>Updates the land scrolling sky.</summary>
    public static RoomMainBehavior ScrollingSkyLand { get; } =
        new ScrollingSkyRoomMain(nameof(ScrollingSkyLand), ScrollingSky.Land, null);

    /// <summary>Updates the ocean scrolling sky.</summary>
    public static RoomMainBehavior ScrollingSkyOcean { get; } =
        new ScrollingSkyRoomMain(nameof(ScrollingSkyOcean), ScrollingSky.Ocean, null);

    /// <summary>Updates the land sky and sets off the escape's random explosions.</summary>
    public static RoomMainBehavior ScrollingSkyLandWithEscapeExplosions { get; } =
        new ScrollingSkyRoomMain(nameof(ScrollingSkyLandWithEscapeExplosions), ScrollingSky.Land,
            EscapeExplosionSite.Outdoors);

    /// <summary>Applies escape shaking and random explosions.</summary>
    public static RoomMainBehavior ShakeScreenWithRandomExplosions { get; } =
        new EscapeRoomMain(nameof(ShakeScreenWithRandomExplosions), EscapeExplosionSite.Outdoors, null);

    /// <summary>Advances the Maridia elevatube room behavior.</summary>
    public static RoomMainBehavior MaridiaElevatube { get; } = new MaridiaElevatubeRoomMain();

    /// <summary>Advances the Ceres shaft elevator behavior.</summary>
    public static RoomMainBehavior CeresElevatorShaft { get; } = new CeresElevatorShaftRoomMain();

    /// <summary>Spawns falling debris in the Ceres pre-elevator hall.</summary>
    public static RoomMainBehavior SpawnCeresPreElevatorHallFallingDebris { get; } = new CeresFallingDebrisRoomMain();

    /// <summary>Advances Ridley's Ceres getaway cutscene.</summary>
    public static RoomMainBehavior CeresRidleyGetaway { get; } = new CeresRidleyGetawayRoomMain();

    /// <summary>Alternates light horizontal and medium diagonal shaking.</summary>
    public static RoomMainBehavior ShakeScreenLightHorizontalAndMediumDiagonal { get; } =
        new EscapeRoomMain(nameof(ShakeScreenLightHorizontalAndMediumDiagonal), EscapeExplosionSite.Indoors,
            ZebesEscapeDefinitions.LightShakingTurnsMediumDiagonal);

    /// <summary>Sets off a random explosion every fourth gameplay update.</summary>
    public static RoomMainBehavior RandomExplosionEveryFourthUpdate { get; } =
        new EscapeRoomMain(nameof(RandomExplosionEveryFourthUpdate), EscapeExplosionSite.Indoors, null);

    /// <summary>Alternates medium horizontal and strong diagonal shaking.</summary>
    public static RoomMainBehavior ShakeScreenMediumHorizontalAndStrongDiagonal { get; } =
        new EscapeRoomMain(nameof(ShakeScreenMediumHorizontalAndStrongDiagonal), EscapeExplosionSite.Indoors,
            ZebesEscapeDefinitions.MediumShakingTurnsStrongDiagonal);

    /// <summary>Applies the Crocomire room's encounter-dependent shaking.</summary>
    public static RoomMainBehavior CrocomireRoomShaking { get; } = new CrocomireRoomShakingRoomMain();

    /// <summary>Every room-main behaviour the room states author, <see cref="None"/> first.</summary>
    public static IReadOnlyList<RoomMainBehavior> All { get; } =
    [
        None, ScrollingSkyLand, ScrollingSkyOcean, ScrollingSkyLandWithEscapeExplosions, ShakeScreenWithRandomExplosions,
        MaridiaElevatube, CeresElevatorShaft, SpawnCeresPreElevatorHallFallingDebris, CeresRidleyGetaway,
        ShakeScreenLightHorizontalAndMediumDiagonal, RandomExplosionEveryFourthUpdate,
        ShakeScreenMediumHorizontalAndStrongDiagonal, CrocomireRoomShaking,
    ];

    /// <summary>The main that runs nothing: the room composes no main.</summary>
    private sealed class NoRoomMain() : RoomMainBehavior(nameof(None))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) { }
    }

    /// <summary>
    /// A main that scrolls a sky behind the room, and for the escape's Landing Site also sets off its outdoor explosions.
    /// The sky's bands drift only where the room's effect or its setup spawns them.
    /// </summary>
    private sealed class ScrollingSkyRoomMain(string name, ScrollingSky sky, EscapeExplosionSite? explosions)
        : RoomMainBehavior(name)
    {
        /// <inheritdoc/>
        public override ScrollingSky? Sky => sky;

        /// <inheritdoc/>
        public override bool SetsOffEscapeExplosions => explosions is not null;

        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) => Ensure.NotNull(scope).ComposeSkyMain(sky, explosions);
    }

    /// <summary>An escape room's main: its explosions, and for two rooms its shaking turning diagonal now and then.</summary>
    private sealed class EscapeRoomMain(string name, EscapeExplosionSite site, EscapeDiagonalShaking? diagonal)
        : RoomMainBehavior(name)
    {
        /// <inheritdoc/>
        public override bool SetsOffEscapeExplosions => true;

        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) => Ensure.NotNull(scope).ComposeEscapeMain(site, diagonal);
    }

    /// <summary>The Maridia elevatube's ride, which the tube's doors start.</summary>
    private sealed class MaridiaElevatubeRoomMain() : RoomMainBehavior(nameof(MaridiaElevatube))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) => Ensure.NotNull(scope).ComposeElevatubeMain();
    }

    /// <summary>
    /// The Ceres elevator shaft's turn and its elevator, composed only when the room is entered through the elevator
    /// door, whose setup starts the turn; entered otherwise the shaft never turns.
    /// </summary>
    private sealed class CeresElevatorShaftRoomMain() : RoomMainBehavior(nameof(CeresElevatorShaft))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope)
        {
            if (Ensure.NotNull(scope).Door.EntersCeresElevatorShaft)
                scope.ComposeElevatorShaftMain();
        }
    }

    /// <summary>The Ceres pre-elevator hall's falling debris, counting down from what the previous room's main left.</summary>
    private sealed class CeresFallingDebrisRoomMain() : RoomMainBehavior(nameof(SpawnCeresPreElevatorHallFallingDebris))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) => Ensure.NotNull(scope).ComposeFallingDebrisMain();
    }

    /// <summary>Ceres Ridley's getaway, which his encounter runs.</summary>
    private sealed class CeresRidleyGetawayRoomMain() : RoomMainBehavior(nameof(CeresRidleyGetaway))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) => Ensure.NotNull(scope).ComposeRidleyGetawayMain();
    }

    /// <summary>The Crocomire room's rumble while his death uncovers the hidden wall.</summary>
    private sealed class CrocomireRoomShakingRoomMain() : RoomMainBehavior(nameof(CrocomireRoomShaking))
    {
        /// <inheritdoc/>
        internal override void Compose(RoomScopeConstruction scope) => Ensure.NotNull(scope).ComposeCrocomireRumbleMain();
    }
}
