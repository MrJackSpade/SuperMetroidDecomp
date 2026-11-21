using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of Samus's ordinary projectiles: a beam, missile, special-attack particle, echo or explosion. Each is created
/// fresh when it takes its place in Samus's shot pool (<see cref="SamusShotPool.Occupy"/>), keeps its own state,
/// and ends when it is deleted, replaced, or Samus's shots are cleared. Nothing it holds reaches the next projectile in
/// its place but two values gameplay carries over: its horizontal position's fraction of a pixel, which is
/// <see cref="LeftBeamStartFromLastShot"/>'s, and, for a beam fired with every place held over the shot in the last place,
/// the count that shot leaves (<see cref="BeamFiredOverLastShot"/>). A place no projectile holds reads as the projectile
/// that last ended there, or one never fired: no occupant, no damage, at rest at the room's origin. What the projectile is
/// belongs to its occupant (<see cref="SamusProjectileOccupant"/>), so an empty place has no type to read.
/// </summary>
public sealed class SamusProjectile
{
    /// <summary>Creates a projectile, not yet fired, in <paramref name="place"/> of the shot pool.</summary>
    internal SamusProjectile(int place) => Place = place;

    /// <summary>
    /// The projectile's place in the shot pool, 0 through 4. Gameplay reads the place: shots take the lowest free one and
    /// update and draw from the highest down, a special attack's particles take the first four, the crash echoes the
    /// last two, and a Super Missile names its link's place (<see cref="SamusShotPool"/>).
    /// </summary>
    [IntegerQuantity("ordinal of the projectile's place in Samus's shot pool")]
    public int Place { get; }

    /// <summary>
    /// The projectile holding the place (#2558), which carries what it is and the damage it deals: from its firing, release
    /// or setting off, which gives it both (<see cref="Occupy"/>), until it ends; null while the place holds no projectile.
    /// </summary>
    public SamusProjectileOccupant? Occupant { get; private set; }

    /// <summary>
    /// Whether the projectile holds its place: from the update it takes the place with its type until it ends. The
    /// firing and the Super Missile's link take the lowest place no projectile holds.
    /// </summary>
    public bool IsOccupied => Occupant is not null;

    /// <summary>
    /// Takes the place with a projectile of <paramref name="type"/> dealing <paramref name="damage"/>, as its firing, release
    /// or setting off does: the damage is the one its type deals, read once the type is known.
    /// </summary>
    /// <param name="type">What the projectile is.</param>
    /// <param name="damage">The damage the projectile deals, one or more.</param>
    /// <returns>The projectile now holding the place.</returns>
    /// <exception cref="InvalidOperationException">A projectile already holds the place.</exception>
    internal SamusProjectileOccupant Occupy(SamusProjectileType type, int damage)
    {
        if (Occupant is { } held)
            throw new InvalidOperationException($"Samus's projectile place {Place} already holds a {held.Type}.");
        Occupant = new SamusProjectileOccupant(this, type, damage);
        return Occupant;
    }

    /// <summary>
    /// The beam combination whose pictures the projectile shows (#1586): the beams Samus had equipped when she fired it,
    /// set off its special attack or released its echo. It stays through every later change to the projectile, its
    /// explosion, an enemy's reflection and the Super Missile's link alike, and through any later change to her
    /// equipment. A Hyper Beam shot is pictured with the Wave and Plasma beams the Hyper Beam equips although it is a
    /// charged Plasma shot, and a missile with the beams beside it although it carries none.
    /// </summary>
    public SamusBeamCombination PicturedBeams { get; internal set; }

    /// <summary>The direction the projectile was fired or reflected in.</summary>
    public SamusProjectileDirection Direction { get; internal set; }

    /// <summary>
    /// Whether the projectile has already struck something: an enemy or enemy-projectile collision spends it
    /// (<see cref="Spend"/>) so later collisions skip it, and the projectile's own update then deletes or converts it.
    /// </summary>
    public bool IsSpent { get; private set; }

    /// <summary>The projectile strikes something (#2562): later collisions skip it, and its own update deletes or converts it.</summary>
    internal void Spend() => IsSpent = true;

    /// <summary>
    /// The projectile flies on as though it had struck nothing (#2562): an enemy it passes through, or a beam combination
    /// that keeps it, leaves it unspent, even when an earlier collision this update spent it.
    /// </summary>
    internal void FlyOn() => IsSpent = false;

    /// <summary>Room position, to a fraction of a pixel.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Velocity in pixels per update.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }
    /// <summary>Collision half-extents in pixels.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>
    /// The projectile's animation (#1600, #2458): the frame it shows, the updates that frame has left, or a still picture it
    /// keeps until its animation changes (#2347), and where its play stands; null while it has none to animate.
    /// </summary>
    internal AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? Animation { get; set; }
    /// <summary>The projectile's animation and where it stands in it; null while it has none to animate.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback => Animation?.Playback;
    /// <summary>The composition the projectile shows, selected by the timed-frame animation or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }

    /// <summary>
    /// The wait before the shot next leaves a trail (#2458): started when a trailing shot is fired or a special attack's
    /// particle starts to trail, counted by its flight, and started again with the flight's interval on each update it
    /// leaves one; null for a shot that leaves no trail, as the Hyper Beam's.
    /// </summary>
    internal UpdateCountdown? Trail { private get; set; }

    /// <summary>
    /// Counts one update of the shot's flight toward its next trail: whether the shot leaves one on this update, after which
    /// the next waits <paramref name="interval"/> updates. A shot that leaves no trail never leaves one.
    /// </summary>
    /// <param name="interval">The updates from this trail to the next, at least one.</param>
    internal bool CountsDownToTrail(int interval)
    {
        if (Trail is not { } trail || !trail.Count())
            return false;
        trail.Restart(Ensure.BetweenInclusive(1, int.MaxValue, interval));
        return true;
    }

    /// <summary>
    /// What the projectile does on its update (#1923), installed when it is fired, released or changed
    /// (<see cref="SamusProjectileOccupant.Install"/>); null for an explosion, which only plays its animation, and before
    /// the projectile is set up.
    /// </summary>
    internal SamusShotBehavior? Behavior { get; set; }

    /// <summary>
    /// The Power Bomb fuse a beam carries, in updates (#2468): none for a beam fired into a free place, and for one fired
    /// with every place held the count the shot it replaced left (<see cref="BeamFiredOverLastShot"/>). The Chainsaw shot
    /// burns it down (<see cref="ChainsawFuse"/>); every other beam, its reflection and its explosion keep it for a beam
    /// fired over it in turn. None for every other projectile, whose kind keeps its own count.
    /// </summary>
    internal UpdateCountdown BeamFuse { get; set; } = UpdateCountdown.Ended();

    /// <summary>
    /// What a beam fired into this projectile's place finds there (#2524), from which <see cref="BeamFiredOverLastShot"/>
    /// gives the beam its Power Bomb fuse: a free place once the projectile has ended, or its kind with the ignition,
    /// ignition speed, link place, departure angle or carried fuse that kind keeps. A beam, the Hyper Beam's shot and their
    /// explosions keep no kind state and carry their fuse; a missile's explosion keeps its missile's.
    /// </summary>
    internal BeamFiredOverLastShot.ReplacedShot AsReplacedByBeam => Occupant is not { } held ? new BeamFiredOverLastShot.ReplacedShot.FreePlace()
        : held.KindState is { } kind ? kind.ReplacedByBeam(ReflectionHeadStart)
        : new BeamFiredOverLastShot.ReplacedShot.Beam(BeamFuse.UpdatesLeft);

    /// <summary>The state the kind of the projectile holding the place keeps, which must be a <typeparamref name="TState"/>.</summary>
    /// <exception cref="InvalidOperationException">No projectile holds the place, or the one holding it keeps no such state.</exception>
    internal TState KindStateAs<TState>() where TState : SamusShotKindState => Occupant switch
    {
        { KindState: TState state } => state,
        { } held => throw new InvalidOperationException(
            $"The projectile in place {Place} keeps no {typeof(TState).Name}; it keeps {held.KindState?.GetType().Name ?? "nothing"}."),
        null => throw new InvalidOperationException($"No projectile holds place {Place}, so none keeps a {typeof(TState).Name}."),
    };

    /// <summary>A live projectile animation marks the projectile drawable and updated.</summary>
    public bool IsActive => Animation is not null;

    /// <summary>
    /// Enemy collision admits a beam, missile, or Super Missile even when it has no animation to play. This
    /// distinction is observable for the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => Occupant is { HasEnemyCollisionPayload: true };

    /// <summary>
    /// The type the projectile strikes blocks with: its own while it holds its place, otherwise the cleared place's
    /// (<see cref="Compatibility.ClearedShotPlaceType"/>), for a block its update reaches after it ended.
    /// </summary>
    internal SamusProjectileType StrikingType =>
        Compatibility.ClearedShotPlaceType.Striking(Occupant, shot => shot.Type, SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false));

    /// <summary>
    /// Ends the projectile: it no longer holds its place, and it reads as no projectile, at rest at the room's origin, to
    /// whatever still holds it and to anything that reads its place before the next projectile takes it.
    /// </summary>
    internal void End()
    {
        Occupant = null;
        PicturedBeams = SamusBeamCombination.Power;
        Direction = default;
        IsSpent = false;
        Position = FixedPosition<RoomPixels>.Zero;
        Velocity = default(PixelVelocity<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Animation = null;
        Sprite = null;
        Trail = null;
        Behavior = null;
        BeamFuse = UpdateCountdown.Ended();
    }
}
