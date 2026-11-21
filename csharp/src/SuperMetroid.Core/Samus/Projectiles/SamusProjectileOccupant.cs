using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile holding one of Samus's shot places (#2558, #2560): what it is and the damage it deals, where it is, how
/// it moves, what it shows and what it does, from its firing, release or setting off until it ends, and what its kind keeps
/// once its behaviour is installed. Only a held place has an occupant, so nothing of a projectile stands for an empty
/// place: a projectile that ends takes all it holds with it. A shot's later changes, its explosion, an enemy's reflection
/// and the Super Missile's link among them, change what it holds.
/// </summary>
[ArchitectureReview("back-reference", "The shot holding a place and the place it holds (#2558): the place's taking creates the occupant, and the place's queries (the type it strikes blocks with, what a beam fired over it finds) read the occupant, so the two share the shot's lifetime.")]
public sealed class SamusProjectileOccupant
{
    /// <summary>
    /// The projectile of <paramref name="type"/> dealing <paramref name="damage"/> taking <paramref name="projectile"/>'s place,
    /// at rest at <paramref name="position"/>.
    /// </summary>
    /// <param name="projectile">The place the projectile holds.</param>
    /// <param name="type">What the projectile is.</param>
    /// <param name="damage">The damage the projectile deals, one or more: its type's.</param>
    /// <param name="position">Where the projectile starts: the room's origin, with any fraction of a pixel its place hands on.</param>
    internal SamusProjectileOccupant(SamusProjectile projectile, SamusProjectileType type, int damage, FixedPosition<RoomPixels> position)
    {
        Projectile = Ensure.NotNull(projectile);
        Type = type;
        Damage = damage;
        Position = position;
    }

    /// <summary>The place the projectile holds.</summary>
    public SamusProjectile Projectile { get; }

    /// <summary>What the projectile is.</summary>
    public SamusProjectileType Type { get; internal set; }

    /// <summary>
    /// The damage the projectile deals, one or more: what its type deals when it takes its place, and what its explosion or
    /// reflection deals once it changes.
    /// </summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(1, int.MaxValue, value);
    }

    /// <summary>
    /// What the projectile's kind keeps beyond what every projectile keeps (#2346), created when the kind's behaviour is
    /// installed (<see cref="Install"/>) and kept through its later behaviours and its explosion; null for a beam, the
    /// Hyper Beam, and before its behaviour is installed.
    /// </summary>
    internal SamusShotKindState? KindState { get; private set; }

    /// <summary>
    /// Whether enemy collision admits the projectile: a beam, missile or Super Missile, even when it has no animation to
    /// play, as the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => Type.Family.IsBeamOrMissile;

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

    /// <summary>Whether the projectile is drawn and updated: it has an animation to play.</summary>
    public bool IsAnimated => Animation is not null;

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
    /// (<see cref="Install"/>); null for an explosion, which only plays its animation, and before the projectile is set up.
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
    /// Gives the projectile its kind's behaviour with the state the kind starts with: firing, a special attack, an
    /// echo's release, a Super Missile's link and a reflection install one. A particle moving on to its next behaviour
    /// keeps its state and changes only <see cref="Behavior"/>.
    /// </summary>
    /// <param name="behavior">The behaviour the projectile runs from its next update.</param>
    /// <param name="kindState">The state the kind keeps, new for this installation, or null for a kind that keeps none.</param>
    internal void Install(SamusShotBehavior behavior, SamusShotKindState? kindState)
    {
        Behavior = Ensure.NotNull(behavior);
        KindState = kindState;
    }
}
