using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of Samus's ordinary projectiles: a beam, missile, special-attack particle, echo or explosion. Each is created
/// fresh when it takes its place in Samus's shot pool (<see cref="ProjectileSlotTable.Occupy"/>), keeps its own state,
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
    /// last two, and a Super Missile names its link's place (<see cref="ProjectileSlotTable"/>).
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
    /// collision walkers skip the projectile, and the projectile's own update then deletes or converts it.
    /// </summary>
    public bool IsSpent { get; internal set; }

    /// <summary>Room-pixel 16.16 position.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Signed 8.8 per-update velocity.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Collision half-extents in pixels.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>The projectile's animation and where it stands in it (#1600); null while it has none to animate.</summary>
    internal AnimationPlayback<SamusProjectileProgram>? Playback { get; set; }
    /// <summary>Updates remaining before the current projectile cue advances.</summary>
    public WrappingCountdown FrameHold { get; internal set; }
    /// <summary>The composition the projectile shows, selected by the timed-frame animation or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Countdown controlling projectile-trail emission.</summary>
    public WrappingCountdown TrailTimer { get; internal set; }

    /// <summary>A missile or Super Missile's ignition.</summary>
    public SamusMissileIgnition MissileIgnition { get; internal set; }

    /// <summary>
    /// The place whose projectile an ignited Super Missile moves as its invisible collision link, or null when it names
    /// none. A Super Missile, its link among them, names <see cref="UnlinkedSuperMissileLink.Place"/> from the update it
    /// is created until it ignites with a link of its own; a reflection leaves it naming none until it re-ignites.
    /// </summary>
    public int? SuperMissileLinkPlace { get; internal set; }

    /// <summary>The orbit angle of an Ice, Plasma or Spazer special-attack particle.</summary>
    public TrigonometryAngle ComboAngle { get; internal set; }

    /// <summary>The homing vertical speed of a Wave special-attack particle.</summary>
    public SignedEightEight WaveComboVerticalSpeed { get; internal set; }

    /// <summary>
    /// The orbit radius, in pixels, of an outward Ice, a Plasma or a Spazer special-attack particle: the distance from
    /// Samus its angle places it at.
    /// </summary>
    public int ComboOrbitRadius { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// The updates an inward Ice or a Wave special-attack particle has left: 600 when the attack starts, counted down
    /// each update until the Ice particle turns outward or the Wave particle ends.
    /// </summary>
    public int ComboLifetime { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The phase of a Plasma special-attack ring.</summary>
    public PlasmaRingPhase PlasmaPhase { get; internal set; }

    /// <summary>
    /// The turn a Spazer special-attack particle makes round its circle each update: its steps on when it turns
    /// clockwise, a full circle less them when it turns counterclockwise.
    /// </summary>
    public TrigonometryAngle SpazerTurnPerUpdate { get; internal set; }

    /// <summary>The phase of a Spazer special-attack particle, set when the attack starts.</summary>
    public SpazerComboPhase SpazerPhase { get; internal set; }

    /// <summary>The part a special-attack particle plays, set when the attack starts (#1683); null for any other projectile.</summary>
    internal SpecialAttackParticle? AttackParticle { get; set; }

    /// <summary>
    /// The Spazer special-attack particle this one is paired with, set when the attack starts (#1683): a beam particle
    /// and the trailing particle that turns the same way. A spent particle ends its partner's place with itself
    /// (<see cref="SpentSpazerParticleEndsPartnerPlace"/>).
    /// </summary>
    internal SamusProjectile? SpazerPartner { get; set; }

    /// <summary>The expanding radius, in pixels, of a Shinespark crash echo.</summary>
    public int EchoRadius { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The departure angle of a Shinespark crash echo, to the angle table's whole steps.</summary>
    public TrigonometryAngle EchoAngle { get; internal set; }

    /// <summary>The Power Bomb fuse the glitched Chainsaw beam runs over its shot, from zero.</summary>
    public WrappingCountdown ChainsawPowerBombFuse { get; internal set; }

    /// <summary>The projectile's frame behaviour.</summary>
    public SamusProjectileFrameBehavior FrameBehavior { get; internal set; }

    /// <summary>A live projectile program marks the projectile drawable and updated.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// Enemy collision admits a beam, missile, or Super Missile even when it has no program to animate. This
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
        MissileIgnition = SamusMissileIgnition.Pending;
        SuperMissileLinkPlace = null;
        ComboAngle = default;
        WaveComboVerticalSpeed = default;
        ComboOrbitRadius = 0;
        ComboLifetime = 0;
        PlasmaPhase = PlasmaRingPhase.Expanding;
        SpazerTurnPerUpdate = TrigonometryAngle.Zero;
        SpazerPhase = SpazerComboPhase.Circling;
        AttackParticle = null;
        SpazerPartner = null;
        EchoRadius = 0;
        EchoAngle = default;
        ChainsawPowerBombFuse = WrappingCountdown.Start(0);
        FrameBehavior = SamusProjectileFrameBehavior.None;
    }
}
