using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of Samus's ordinary projectiles: a beam, missile, special-attack particle, echo or explosion. Each is created
/// fresh when it takes its place in Samus's shot pool (<see cref="SamusShotPool.Occupy"/>), keeps its own state,
/// and ends when it is deleted, replaced, or Samus's shots are cleared. Nothing it holds reaches the next projectile in
/// its place; the one value gameplay carries from place to place, the horizontal sub-pixel position, is
/// <see cref="LeftMuzzleSubpixelCarry"/>'s. A place no projectile holds reads as the projectile that last ended there,
/// or one never fired: no type, no damage, at rest at the room's origin.
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

    /// <summary>The damage the projectile deals; zero before it is fired and once it ends.</summary>
    private int _damage;

    /// <summary>
    /// Whether the projectile holds its place: from the update it takes the place with its damage until it ends. The
    /// firing and the Super Missile's link take the lowest place no projectile holds.
    /// </summary>
    public bool IsOccupied { get; private set; }

    /// <summary>
    /// The damage the projectile deals, one or more, or zero before it is fired and once it ends. Every projectile takes
    /// its place with its damage, so setting it marks the place held.
    /// </summary>
    public int Damage
    {
        get => _damage;
        internal set
        {
            _damage = Ensure.BetweenInclusive(1, int.MaxValue, value);
            IsOccupied = true;
        }
    }

    /// <summary>What the projectile is; <see cref="SamusProjectileType.None"/> before it is fired and once it ends.</summary>
    public SamusProjectileType Type { get; internal set; }

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
    /// Whether the projectile has already struck something: enemy and enemy-projectile collision set it so later
    /// collisions skip the projectile, and the projectile's own update then deletes or converts it.
    /// </summary>
    public bool IsSpent { get; internal set; }

    /// <summary>Room position in 1/65536 pixels.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Signed 1/256-pixel per-update velocity.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Collision half-extents in pixels.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>The projectile's animation and where it stands in it (#1600); null while it has none to animate.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback { get; set; }
    /// <summary>
    /// Updates remaining before the projectile's animation shows its next frame; none once it shows a still picture it keeps
    /// until its animation changes (#2347).
    /// </summary>
    public WrappingCountdown? FrameHold { get; internal set; }
    /// <summary>The composition the projectile shows, selected by the timed-frame animation or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Countdown controlling projectile-trail emission.</summary>
    public WrappingCountdown TrailTimer { get; internal set; }

    /// <summary>
    /// What the projectile does on its update (#1923), installed when it is fired, released or changed; null for an
    /// explosion, which only plays its animation, and before the projectile is set up.
    /// </summary>
    internal SamusShotBehavior? Behavior { get; set; }

    /// <summary>
    /// What the projectile's kind keeps beyond what every projectile keeps (#2346), created when the kind's behaviour is
    /// installed (<see cref="Install"/>) and kept through its later behaviours and its explosion; null for a beam, the
    /// Hyper Beam, and before the projectile is set up.
    /// </summary>
    internal SamusShotKindState? KindState { get; private set; }

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

    /// <summary>The state the projectile's kind keeps, which must be a <typeparamref name="TState"/>.</summary>
    /// <exception cref="InvalidOperationException">The projectile keeps no such state.</exception>
    internal TState KindStateAs<TState>() where TState : SamusShotKindState =>
        KindState as TState ?? throw new InvalidOperationException(
            $"The projectile in place {Place} keeps no {typeof(TState).Name}; it keeps {KindState?.GetType().Name ?? "nothing"}.");

    /// <summary>A live projectile animation marks the projectile drawable and updated.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// Enemy collision admits a beam, missile, or Super Missile even when it has no animation to play. This
    /// distinction is observable for the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => !Type.IsNone && Type.Family.IsBeamOrMissile;

    /// <summary>
    /// Ends the projectile: it no longer holds its place, and it reads as no projectile, at rest at the room's origin, to
    /// whatever still holds it and to anything that reads its place before the next projectile takes it.
    /// </summary>
    internal void End()
    {
        _damage = 0;
        IsOccupied = false;
        Type = SamusProjectileType.None;
        PicturedBeams = SamusBeamCombination.Power;
        Direction = default;
        IsSpent = false;
        Position = FixedPosition<RoomPixels>.Zero;
        Velocity = default(EightEightVector<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Playback = null;
        FrameHold = WrappingCountdown.Start(0);
        Sprite = null;
        TrailTimer = WrappingCountdown.Start(0);
        Behavior = null;
        KindState = null;
    }
}
