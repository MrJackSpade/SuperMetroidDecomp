using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's encounter-wide state and the values of his body. The four visible enemy parts remain physical <see cref="RoomEnemyPlace"/> instances; the eye's values live
/// in its own <see cref="DraygonEyeState"/>.
/// </summary>
public sealed class DraygonEnemyState : SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing
{
    /// <inheritdoc />
    Position16<RoomPixels>? SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyPlacement => BodyPlacement;

    /// <summary>His body shows nothing once his body is deleted, as his room keeps showing it (#2194).</summary>
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyHidden => Body.Presence.HasAny(EnemyPresence.Deleted);

    /// <summary>Anchors the encounter-wide state to Draygon's physical body place.</summary>
    internal DraygonEnemyState(RoomEnemyPlace body) => Body = body;

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

    /// <summary>His body, which owns encounter movement, health, and the body's phases, and runs first.</summary>
    public RoomEnemyPlace Body { get; }
    /// <summary>Each part's playing animation and where it stands, keyed by the part.</summary>
    private readonly Dictionary<RoomEnemyPlace, AnimationPlayback<DraygonSegment>> _playbacks = new(ReferenceEqualityComparer.Instance);

    /// <summary><paramref name="part"/>'s playing animation and where it stands.</summary>
    internal AnimationPlayback<DraygonSegment> Playback(RoomEnemyPlace part) =>
        _playbacks.TryGetValue(part, out AnimationPlayback<DraygonSegment> playback)
            ? playback
            : throw new InvalidOperationException("A Draygon part has no animation.");

    /// <summary>Sets <paramref name="part"/>'s playing animation and where it stands.</summary>
    internal void SetPlayback(RoomEnemyPlace part, AnimationPlayback<DraygonSegment> playback) => _playbacks[Ensure.NotNull(part)] = playback;

    /// <summary>Starts the animation beginning at <paramref name="segment"/> on <paramref name="part"/>, before its first play.</summary>
    internal void StartAnimation(RoomEnemyPlace part, DraygonSegment segment) =>
        SetPlayback(part, new AnimationPlayback<DraygonSegment>(DraygonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation));

    /// <summary>His eye, attached by its initializer; tracks Samus using coordinates copied from the body.</summary>
    public RoomEnemyPlace Eye { get => field ?? throw new InvalidOperationException("Draygon's eye part is not attached; part initialization attaches it."); internal set; }
    /// <summary>His tail, attached by its initializer; its animations drive whips and the final release handshake.</summary>
    public RoomEnemyPlace Tail { get => field ?? throw new InvalidOperationException("Draygon's tail part is not attached; part initialization attaches it."); internal set; }
    /// <summary>His arms, attached by their initializer; their animation follows the body's authoritative coordinates.</summary>
    public RoomEnemyPlace Arms { get => field ?? throw new InvalidOperationException("Draygon's arms part is not attached; part initialization attaches it."); internal set; }



    /// <summary>
    /// The off-screen point at the left where Draygon is placed between attack rounds: the body's position when the
    /// intro delay ends. Its Y is the common reset height, which also bounds how high a swoop path climbs (compared
    /// by the wrapped signed difference) and where a round that chose another swoop restarts.
    /// </summary>
    public Position16<RoomPixels> LeftSideReset { get; internal set; }

    /// <summary>
    /// The X of the off-screen place at the right (#2221), 672 pixels right of <see cref="LeftSideReset"/>, wrapping: a
    /// leftward swoop aims from it and a leftward goop pass starts there.
    /// </summary>
    public int RightSideResetX { get; internal set; }

    /// <summary>
    /// The downward acceleration a swoop path is built with, in pixels per update per update (#2221). It starts at
    /// 24/256, each eye hit adds 8/256 while the result stays below 160/256, and a tail-whip hit on grabbed Samus restores
    /// 24/256.
    /// </summary>
    public SignedEightEight SwoopAcceleration { get; internal set; }

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
    /// How far the tail whip shifts the body's picture against the body, in room pixels, without moving the body or its
    /// parts (#1940): his body is placed this far from his position (#2031).
    /// </summary>
    public Offset16<RoomPixels> BodyPictureShift { get; internal set; }

    /// <summary>Where his body is placed (#2031): his position, shifted by <see cref="BodyPictureShift"/>.</summary>
    public Position16<RoomPixels> BodyPlacement => Body.Position.Whole + BodyPictureShift;

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


    /// <summary>
    /// Horizontal radius of the carry spiral in 1/65536 pixels; the vertical radius is a quarter of its whole pixels.
    /// It is cleared by a grab and grows by one eighth of a pixel per spiral frame, so its whole part advances every
    /// eighth frame; the spiral ends when the whole part reaches 160.
    /// </summary>
    public SignedSixteenSixteen SpiralXRadius { get; internal set; }

    /// <summary>
    /// Center point of the carry spiral, initialized to (256, 384).
    /// The Y axis carries its fractional part: each spiral frame lifts it a quarter pixel, the center rising one pixel
    /// every four. X has no fractional part and
    /// stays whole. A grab keeps the part of a pixel the last spiral left (<see cref="DraygonRegrabSpiralLift"/>).
    /// </summary>
    public DraygonRegrabSpiralLift SpiralCenter { get; internal set; }

    /// <summary>
    /// Draygon's position around the carry spiral, in 256ths of a turn: three quarters when a grab starts, advanced by
    /// the whole steps of <see cref="SpiralAngleDelta"/> each spiral frame and wrapping around the turn.
    /// </summary>
    public RotationAngle SpiralAngle { get; internal set; }
    /// <summary>
    /// Angular step of the carry spiral per frame: eight steps when a grab starts, slowed by 1/256 of a step each
    /// spiral frame before it is applied, so the orbit decelerates as the radius grows.
    /// </summary>
    public RotationAngle SpiralAngleDelta { get; internal set; }

    /// <summary>Frames left of an incidental 64-frame tail whip.</summary>
    public int TailWhipTimer { get; internal set; }



    /// <summary>
    /// The installed health-shade band 0-7; null until a band is installed.
    /// </summary>
    public int? HealthBand { get; internal set; }

    /// <summary>True after terminal burial records the area boss as defeated; prevents repeating that persistence write.</summary>
    public bool BossDefeatPersisted { get; internal set; }


    /// <summary>
    /// The body's phase; the body runs it each update before its three parts take the body's position. The fight starts
    /// in its opening delay.
    /// </summary>
    public DraygonPhase Phase { get; internal set; } = DraygonPhase.IntroInitialDelay;

    /// <summary>The way Draygon's swoop or goop pass travels (#2165).</summary>
    public HorizontalDirection PassDirection { get; internal set; }

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
