using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's encounter-wide state and the values of his body. The four visible enemy parts remain physical <see cref="RoomEnemySlot"/> instances; the eye's values live
/// in its own <see cref="DraygonEyeState"/>.
/// </summary>
public sealed class DraygonEnemyState
{
    /// <summary>Anchors the encounter-wide state to Draygon's physical body slot.</summary>
    internal DraygonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Draygon's body layer's and sprites' colours (#1512).</summary>
    internal DraygonColours Colours { get; } = new();

    /// <summary>Gives his body and his attached tail and arms the shading his colours show now (#1512).</summary>
    internal void ShadeSprites()
    {
        PictureShading sprites = Colours.Sprites;
        Body.Shading = sprites;
        Tail.Shading = sprites;
        Arms.Shading = sprites;
    }

    /// <summary>His body, which owns encounter movement, health, and the body-function dispatcher, and runs first.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Each part's playing animation and where it stands, keyed by the part's record.</summary>
    private readonly Dictionary<RoomEnemySlot, AnimationPlayback<DraygonSegment>> _playbacks = new(ReferenceEqualityComparer.Instance);

    /// <summary><paramref name="part"/>'s playing animation and where it stands.</summary>
    internal AnimationPlayback<DraygonSegment> Playback(RoomEnemySlot part) =>
        _playbacks.TryGetValue(part, out AnimationPlayback<DraygonSegment> playback)
            ? playback
            : throw new InvalidOperationException("A Draygon part has no animation.");

    /// <summary>Sets <paramref name="part"/>'s playing animation and where it stands.</summary>
    internal void SetPlayback(RoomEnemySlot part, AnimationPlayback<DraygonSegment> playback) => _playbacks[Ensure.NotNull(part)] = playback;

    /// <summary>Starts the animation beginning at <paramref name="segment"/> on <paramref name="part"/>, before its first play.</summary>
    internal void StartAnimation(RoomEnemySlot part, DraygonSegment segment) =>
        SetPlayback(part, new AnimationPlayback<DraygonSegment>(DraygonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation));

    /// <summary>His eye, attached by its initializer; tracks Samus using coordinates copied from the body.</summary>
    public RoomEnemySlot Eye { get => field ?? throw new InvalidOperationException("Draygon's eye part is not attached; part initialization attaches it."); internal set; }
    /// <summary>His tail, attached by its initializer; its animation scripts drive whips and the final release handshake.</summary>
    public RoomEnemySlot Tail { get => field ?? throw new InvalidOperationException("Draygon's tail part is not attached; part initialization attaches it."); internal set; }
    /// <summary>His arms, attached by their initializer; their animation follows the body's authoritative coordinates.</summary>
    public RoomEnemySlot Arms { get => field ?? throw new InvalidOperationException("Draygon's arms part is not attached; part initialization attaches it."); internal set; }


    /// <summary>The boss-room load disables the normal minimap and marks its four tiles.</summary>
    public bool MinimapDisabledAndBossTilesExplored { get; internal set; }

    /// <summary>
    /// The off-screen point at the left where Draygon is placed between attack rounds: the body's position when the
    /// intro delay ends. Its Y is the common reset height, which also bounds how high a swoop path climbs (compared
    /// by the wrapped signed difference) and where a round that chose another swoop restarts.
    /// </summary>
    public Position16<RoomPixels> LeftSideReset { get; internal set; }

    /// <summary>
    /// The off-screen point at the right, 672 pixels right of <see cref="LeftSideReset"/> at the same height. Only the X
    /// axis is read: a leftward swoop aims from it and a leftward goop pass starts there.
    /// </summary>
    public Position16<RoomPixels> RightSideReset { get; internal set; }

    /// <summary>
    /// Downward 8.8 acceleration a swoop path is built with, in 1/256 pixel per frame per frame. Only the Y axis is
    /// meaningful. It starts at 24, each eye hit adds 8 while the result stays below 160, and a tail-whip hit on
    /// grabbed Samus restores 24.
    /// </summary>
    public EightEightVector<RoomPixels> SwoopAcceleration { get; internal set; }

    /// <summary>
    /// How many updates the intro dance has run: zero when the encounter starts and one more after each of the dance's
    /// 1,232 updates. Each Evir takes the traced path's step this far past its own start delay, and waits while it has
    /// not reached the delay.
    /// </summary>
    public int IntroDanceStep { get; internal set; }

    /// <summary>The Evirs the intro spawned for its dance, in the order they spawned, each with its own start delay.</summary>
    internal List<DraygonIntroDancer> IntroDancers { get; } = [];

    /// <summary>The Evirs his death spawned to bury him, in the order they spawned, each with its own burial entry.</summary>
    internal List<DraygonBurialEvir> BurialEvirs { get; } = [];

    /// <summary>
    /// Signed pixel offset written by the tail-whip step and added to the body's
    /// layer scroll, displacing the body graphics without moving the enemy records.
    /// </summary>
    public Offset16<BackgroundPixels> BodyGraphicsDisplacement { get; internal set; }

    /// <summary>
    /// Where Draygon's body lies in the room (#1354, #1617): the room pixel his body's parts' area's top-left corner lies on,
    /// derived after enemies draw from the body position and <see cref="BodyGraphicsDisplacement"/>, which moves the body's
    /// pictures against the body.
    /// </summary>
    public Position16<RoomPixels> BodyOrigin { get; internal set; }

    /// <summary>
    /// The path of the swoop Draygon is flying or last flew: none until he plans his first swoop, then the heights from
    /// the swoop's lowest point up toward the reset height and the height he started it at.
    /// </summary>
    internal DraygonSwoopPath? SwoopPath { get; set; }

    /// <summary>Whether Draygon faces right; false faces left.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>
    /// Phase of the goop pass's vertical bob, in 256ths of a turn: zero when a pass starts and one unit further each
    /// movement update, wrapping around the turn, which the bob depends on. The body's height is 384 plus the cosine
    /// of this phase times 32.
    /// </summary>
    public RotationAngle GoopYOscillationAngle { get; internal set; }

    /// <summary>Goop shots left in a firing pass, set to sixteen when the pass begins; the pass ends when it reaches zero.</summary>
    public int GoopCounter { get; internal set; }

    /// <summary>Count of wall-turret projectiles admitted to the shared pool; disabled cannons and exhausted allocations do not increment it.</summary>
    public int WallTurretsSpawned { get; internal set; }
    /// <summary>Count of goop projectiles admitted by the facing-specific body-list producers.</summary>
    public int GoopProjectilesSpawned { get; internal set; }

    /// <summary>
    /// Horizontal radius of the carry spiral in 16.16 pixels; the vertical radius is a quarter of its whole pixels.
    /// It is cleared by a grab and grows by one eighth of a pixel per spiral frame, so its whole part advances every
    /// eighth frame; the spiral ends when the whole part reaches 160.
    /// </summary>
    public SignedSixteenSixteen SpiralXRadius { get; internal set; }

    /// <summary>
    /// Center point of the carry spiral, initialized to (256, 384).
    /// The Y axis carries its fractional part: retail subtracts a quarter pixel each
    /// spiral frame, lifting the center by one pixel every four. X has no fractional part and
    /// stays whole.
    /// </summary>
    public FixedPosition<RoomPixels> SpiralCenter { get; internal set; }

    /// <summary>
    /// Draygon's position around the carry spiral, in 256ths of a turn: three quarters when a grab starts, advanced by
    /// the whole table units of <see cref="SpiralAngleDelta"/> each spiral frame and wrapping around the turn.
    /// </summary>
    public RotationAngle SpiralAngle { get; internal set; }
    /// <summary>
    /// Angular step of the carry spiral per frame: eight table units when a grab starts, slowed by 1/256 unit each
    /// spiral frame before it is applied, so the orbit decelerates as the radius grows.
    /// </summary>
    public RotationAngle SpiralAngleDelta { get; internal set; }

    /// <summary>Frames left of an incidental 64-frame tail whip.</summary>
    public int TailWhipTimer { get; internal set; }

    /// <summary>How many times Draygon's chase has reached its grab window and caught Samus.</summary>
    public int SuccessfulGrabs { get; internal set; }

    /// <summary>Number of tail-hit steps that actually damaged grabbed Samus.</summary>
    public int TailWhipHits { get; internal set; }


    /// <summary>
    /// The installed health-palette band 0-7; null until a band is installed.
    /// </summary>
    public int? HealthBand { get; internal set; }

    /// <summary>True after the burial at the end of the fight scatters sixteen physical pickups using the body's drop definition.</summary>
    public bool ItemDropRequested { get; internal set; }
    /// <summary>True after terminal burial invokes the persistent area-boss-defeated callback; prevents repeating that persistence write.</summary>
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>Number of exact 64-frame turret-selection RNG draws observed.</summary>
    public int TurretCadenceChecks { get; internal set; }

    /// <summary>
    /// The wall cannons disabled by Draygon setup or a destroyed room object.
    /// This is the shared state Draygon's firing selector observes.
    /// </summary>
    internal HashSet<DraygonWallCannon> DisabledCannons { get; } = [];

    /// <summary>The body function; body main AI dispatches this before copying coordinates to the three parts.</summary>
    public DraygonAiFunction Function { get; internal set; }

    /// <summary>
    /// The updates the current intro stage has run: zero when the opening delay starts and when the dance starts, and
    /// one more after each of that stage's updates. The opening delay ends at 256 and the dance at its duration.
    /// </summary>
    /// <remarks>
    /// This count and <see cref="BurialWait"/> are never read under each other's meaning (#1476): each intro stage ends by
    /// zeroing the count, and the burial wait is loaded with its full length when the dying body reaches its burial point,
    /// so they are held apart.
    /// </remarks>
    public int IntroFrames
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, DraygonIntroDanceDefinitions.DurationFrames, value);
    }

    /// <summary>The updates the dying body waits at its burial point before it sinks, counted down to zero.</summary>
    public int BurialWait { get; internal set; }

    /// <summary>
    /// The swoop path entry whose height the body takes next: it counts down from the path's
    /// <see cref="DraygonSwoopPath.EntryCount"/> to zero on the descent and back up on the ascent.
    /// </summary>
    internal int SwoopEntry
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>The horizontal speed the swoop and goop passes move the body's X by, toward their side.</summary>
    internal UnsignedSixteenSixteen SwoopVelocity { get; set; }
}
