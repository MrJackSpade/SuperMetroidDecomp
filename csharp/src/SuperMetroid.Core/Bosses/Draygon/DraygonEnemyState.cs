using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's encounter-wide state and the values of his body. His four parts, body, eye, tail and arms, are each its own
/// enemy made with the encounter by his body's setup and keeping its own animation (#2298); the eye's tracking lives in
/// <see cref="EyeTracking"/>.
/// </summary>
public sealed class DraygonEnemyState : SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing
{
    /// <inheritdoc />
    Position16<RoomPixels>? SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyPlacement => BodyPlacement;

    /// <summary>His body shows nothing once his body is deleted, as his room keeps showing it (#2194).</summary>
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyHidden => Body.Presence.HasAny(EnemyPresence.Deleted);

    /// <summary>
    /// Begins the encounter of the body at <paramref name="body"/> and the parts his room lists with it, each playing the
    /// animation its setup starts, in the health band his health starts in (#2304).
    /// </summary>
    /// <param name="body">The enemy holding his body's place, which begins the encounter as it sets up.</param>
    /// <param name="eye">The place his eye occupies.</param>
    /// <param name="tail">The place his tail occupies.</param>
    /// <param name="arms">The place his arms occupy.</param>
    internal DraygonEnemyState(RoomEnemyOccupant body, RoomEnemyPlace eye, RoomEnemyPlace tail, RoomEnemyPlace arms)
    {
        BodyOccupant = Ensure.NotNull(body);
        BodyPart = new DraygonPart(this, body.Place, DraygonSegment.BodyFacingLeftIdle);
        EyePart = new DraygonPart(this, eye, DraygonSegment.EyeFacingLeftIdle);
        TailPart = new DraygonPart(this, tail, DraygonSegment.TailFacingLeftInitialFakeWhip);
        ArmsPart = new DraygonPart(this, arms, DraygonSegment.ArmsFacingLeftIdle);
        HealthBand = DraygonHealthBand.Of(body.Place.Health);
    }

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

    /// <summary>The enemy holding his body's place, whose definition's damage his tail's whip deals (#2558).</summary>
    public RoomEnemyOccupant BodyOccupant { get; }

    /// <summary>His body, which owns encounter movement, health, and the body's phases, and runs first.</summary>
    public DraygonPart BodyPart { get; }

    /// <summary>His eye, which tracks Samus using coordinates copied from the body.</summary>
    public DraygonPart EyePart { get; }

    /// <summary>His tail, whose animations drive his whips and his release of Samus after the final whips.</summary>
    public DraygonPart TailPart { get; }

    /// <summary>His arms, whose animation follows the body's authoritative coordinates.</summary>
    public DraygonPart ArmsPart { get; }

    /// <summary>The place his body occupies.</summary>
    public RoomEnemyPlace Body => BodyPart.Place;

    /// <summary>The place his eye occupies.</summary>
    public RoomEnemyPlace Eye => EyePart.Place;

    /// <summary>The place his tail occupies.</summary>
    public RoomEnemyPlace Tail => TailPart.Place;

    /// <summary>The place his arms occupy.</summary>
    public RoomEnemyPlace Arms => ArmsPart.Place;

    /// <summary>His eye's tracking: inert until his body publishes the way he faces, then tracking Samus.</summary>
    internal DraygonEyeState EyeTracking { get; } = new(DraygonEyeAction.Inert);



    /// <summary>
    /// The off-screen point at the left where Draygon is placed between attack rounds: the body's position when the
    /// intro delay ends. Its Y is the common reset height, which also bounds how high a swoop path climbs (compared
    /// as <see cref="DistanceAcrossRoomEdge.LiesBefore"/> compares rows) and where a round that chose another swoop restarts.
    /// </summary>
    public Position16<RoomPixels> LeftSideReset { get; internal set; }

    /// <summary>
    /// The X of the off-screen place at the right (#2221), <see cref="DraygonArenaDefinitions.RightSideResetDistance"/> right of <see cref="LeftSideReset"/>, wrapping: a
    /// leftward swoop aims from it and a leftward goop pass starts there.
    /// </summary>
    public int RightSideResetX { get; internal set; }

    /// <summary>
    /// The downward acceleration a swoop path is built with, in pixels per update per update (#2221). It starts at its
    /// starting value, each eye hit adds its step while the result stays below its limit, and a tail-whip hit on grabbed
    /// Samus restores the starting value (<see cref="DraygonSwoopDefinitions"/>).
    /// </summary>
    public SignedEightEight SwoopAcceleration { get; internal set; }

    /// <summary>The Evirs the intro spawned for its dance, in the order they spawned, each with its own start delay.</summary>
    internal List<DraygonIntroDancer> IntroDancers { get; } = [];

    /// <summary>The Evirs his death spawned to bury him, in the order they spawned, each with its own burial entry.</summary>
    internal List<DraygonBurialEvir> BurialEvirs { get; } = [];

    /// <summary>
    /// How far the tail whip shifts the body's picture against the body, in room pixels, without moving the body or its
    /// parts (#1940): his body is placed this far from his position (#2031).
    /// </summary>
    internal Offset16<RoomPixels> BodyPictureShift { private get; set; }

    /// <summary>Where his body is placed (#2031): his position, shifted by <see cref="BodyPictureShift"/>.</summary>
    public Position16<RoomPixels> BodyPlacement => Body.Position.Whole + BodyPictureShift;

    /// <summary>
    /// The path of the swoop Draygon is flying or last flew: none until he plans his first swoop, then the heights from
    /// the swoop's lowest point up toward the reset height and the height he started it at.
    /// </summary>
    internal DraygonSwoopPath? SwoopPath { get; set; }

    /// <summary>The way Draygon faces: left until his first rightward pass turns him.</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// Where the goop pass's vertical bob stands: zero when a pass starts and a step further each movement update, round
    /// and round the turn; his body sways up and down about his swim line by it, as far as
    /// <see cref="DraygonArenaDefinitions.GoopSwayRadius"/>.
    /// </summary>
    public RotationAngle GoopYOscillationAngle { get; internal set; }

    /// <summary>The goop shots left in a firing pass, <see cref="DraygonArenaDefinitions.GoopsPerPass"/> when the pass begins; the pass ends on the last of them.</summary>
    public BossCountdown GoopShotsLeft { get; } = new();


    /// <summary>
    /// Horizontal radius of the carry spiral; the vertical radius is a quarter of its whole pixels. A grab clears it and
    /// each spiral frame grows it by <see cref="DraygonGrabDefinitions.SpiralRadiusGrowth"/>; the spiral ends when its whole
    /// pixels reach <see cref="DraygonGrabDefinitions.MaximumSpiralRadius"/>.
    /// </summary>
    public SignedSixteenSixteen SpiralXRadius { get; internal set; }

    /// <summary>
    /// Center point of the carry spiral, initialized to <see cref="DraygonGrabDefinitions.SpiralTarget"/>.
    /// The Y axis carries its fractional part: each spiral frame lifts it by <see cref="DraygonGrabDefinitions.SpiralCenterLift"/>.
    /// X has no fractional part and
    /// stays whole. A grab keeps the part of a pixel the last spiral left (<see cref="DraygonRegrabSpiralLift"/>).
    /// </summary>
    public DraygonRegrabSpiralLift SpiralCenter { get; internal set; }

    /// <summary>
    /// Draygon's position around the carry spiral: <see cref="DraygonGrabDefinitions.InitialSpiralAngle"/> when a grab starts, advanced by
    /// the whole steps of <see cref="SpiralAngleDelta"/> each spiral frame and wrapping around the turn.
    /// </summary>
    public RotationAngle SpiralAngle { get; internal set; }
    /// <summary>
    /// Angular step of the carry spiral per frame: eight steps when a grab starts, slowed by
    /// <see cref="DraygonGrabDefinitions.SpiralTurnSlowing"/> each spiral frame before it is applied, so the orbit decelerates as the radius grows.
    /// </summary>
    public RotationAngle SpiralAngleDelta { get; internal set; }

    /// <summary>
    /// The pause his tail whip makes in his spiral (#2538): it starts on the pause's first update, lasts
    /// <see cref="DraygonGrabDefinitions.TailWhipUpdates"/>, and his spiral resumes on its last.
    /// </summary>
    public BossCountdown TailWhipPause { get; } = new();



    /// <summary>
    /// The health band his colours show (#2266): the band his health starts in (#2304), until a reaction finds his health
    /// in another. His body layer shows it from his first hurt update or that reaction, never from his setup (#2547).
    /// </summary>
    public DraygonHealthBand HealthBand { get; internal set => field = Ensure.NotNull(value); }

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
    /// The current intro stage: the opening delay of <see cref="DraygonArenaDefinitions.IntroUpdates"/>, then the dance of
    /// <see cref="DraygonIntroDanceDefinitions.DurationFrames"/>, each counted once on each of its updates. In the dance,
    /// each Evir takes the traced path's step as far past its own start delay as the stage's age, and waits while the
    /// stage has not reached the delay.
    /// </summary>
    public UpdateCountdown IntroStage { get; internal set; } = UpdateCountdown.Lasting(DraygonArenaDefinitions.IntroUpdates);

    /// <summary>
    /// The dying body's wait at its burial point before it sinks (#2538): reaching the point starts it for
    /// <see cref="DraygonArenaDefinitions.BurialWaitUpdates"/>, and the body starts sinking on its last update.
    /// </summary>
    public BossCountdown BurialWait { get; } = new();

    /// <summary>
    /// The half of the swoop the body is flying, one update a path entry: the descent takes the heights from the path's
    /// <see cref="DraygonSwoopPath.EntryCount"/> down by its updates left and ends at the bottom; the climb takes them back
    /// up from the first by its age and ends at the top.
    /// </summary>
    internal UpdateCountdown SwoopHalf { get; set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The horizontal velocity the swoop and goop passes move the body's X by each update (#2569): toward the pass's side,
    /// so a leftward pass's velocity is negative.
    /// </summary>
    internal SignedSixteenSixteen SwoopVelocity { get; set; }
}
