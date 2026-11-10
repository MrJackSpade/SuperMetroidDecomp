using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One of Samus's five ordinary projectile slots: a beam, missile, special-attack particle, echo or explosion.</summary>
public sealed class SamusProjectileSlot
{
    /// <summary>Creates the slot at its position in the projectile table.</summary>
    internal SamusProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>The slot's ordinal in the projectile table, 0 through 4.</summary>
    [IntegerQuantity("ordinal of the slot in the projectile table")]
    public int SlotIndex { get; }

    /// <summary>The damage of the projectile holding the slot; zero while the slot is free.</summary>
    private int _damage;

    /// <summary>
    /// Whether a projectile holds the slot: from the update a projectile takes it with its damage until the slot is
    /// cleared. The firing routines and the Super Missile's link take the lowest slot no projectile holds.
    /// </summary>
    public bool IsOccupied { get; private set; }

    /// <summary>
    /// The damage the projectile holding the slot deals, 1 through 65535, or zero while the slot is free. Every
    /// projectile takes its slot with its damage, so setting it marks the slot occupied.
    /// </summary>
    public int Damage
    {
        get => _damage;
        internal set
        {
            _damage = Ensure.BetweenInclusive(1, ushort.MaxValue, value);
            IsOccupied = true;
        }
    }

    /// <summary>What the slot holds; <see cref="SamusProjectileType.None"/> marks an empty slot.</summary>
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
    /// <summary>Where the slot's projectile program resumes; null marks the slot inactive for animation.</summary>
    internal AnimationProgramCursor<SamusProjectileProgram>? Cursor { get; set; }
    /// <summary>Updates remaining before the current projectile instruction advances.</summary>
    public WrappingCountdown FrameHold { get; internal set; }
    /// <summary>The composition the projectile shows, selected by the timed-frame interpreter or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>The animation frame the projectile's current record shows, zero or more.</summary>
    public int AnimationFrame
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>Countdown controlling projectile-trail emission.</summary>
    public WrappingCountdown TrailTimer { get; internal set; }

    /// <summary>A missile or Super Missile's ignition.</summary>
    public SamusMissileIgnition MissileIgnition { get; internal set; }

    /// <summary>
    /// The slot an ignited Super Missile moves as its invisible collision link, or null when it names none. An unlinked
    /// Super Missile names slot zero: a missile that ignites with no free slot, or is reflected before igniting, treats
    /// slot zero as its link, as the game does. A reflection leaves it naming none until it re-ignites.
    /// </summary>
    public int? SuperMissileLinkSlot { get; internal set; }

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

    /// <summary>The signed angle step, in table units, a Spazer special-attack particle turns by each update.</summary>
    public int SpazerAngleStep { get; internal set; }

    /// <summary>The expanding radius, in pixels, of a Shinespark crash echo that holds the slot.</summary>
    public int EchoRadius { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The departure angle of a Shinespark crash echo, to the angle table's whole steps.</summary>
    public TrigonometryAngle EchoAngle { get; internal set; }

    /// <summary>The Power Bomb fuse the glitched Chainsaw beam runs over its shot.</summary>
    public WrappingCountdown ChainsawPowerBombFuse { get; internal set; }

    /// <summary>The Spazer special attack's particle phase; it survives slot reuse.</summary>
    public SpazerComboPhase AuxiliaryPhase { get; internal set; }
    /// <summary>The slot's projectile frame behaviour.</summary>
    public SamusProjectileFrameBehavior FrameBehavior { get; internal set; }

    /// <summary>A live projectile program marks the slot allocated and drawable.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>
    /// Enemy collision admits a beam, missile, or Super Missile even when the slot has no program to animate. This
    /// distinction is observable for the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => !Type.IsNone && Type.Family.IsBeamOrMissile;

    /// <summary>Clears the per-projectile state deletion clears; trail and Spazer phase state survive reuse.</summary>
    internal void ClearFields()
    {
        Vacate();
        Type = SamusProjectileType.None;
        PicturedBeams = SamusBeamCombination.Power;
        Direction = default;
        IsSpent = false;
        Position = FixedPosition<RoomPixels>.Zero;
        Velocity = default(EightEightVector<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Cursor = null;
        FrameHold = WrappingCountdown.Start(0);
        Sprite = null;
        AnimationFrame = 0;
        ClearKindState();
        FrameBehavior = SamusProjectileFrameBehavior.None;
    }

    /// <summary>
    /// The per-slot state an elevator ride or a full projectile reset clears. Unlike deletion it keeps the sub-pixel position, which a later beam
    /// fired left from the slot reads at its muzzle check, the animation frame and the Spazer phase, which every Spazer
    /// special attack sets before reading.
    /// </summary>
    internal void ClearForProjectileDataReset()
    {
        TrailTimer = WrappingCountdown.Start(0);
        Position = Position.WithWhole(Position16<RoomPixels>.Zero);
        Direction = default;
        IsSpent = false;
        Velocity = default(EightEightVector<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Type = SamusProjectileType.None;
        PicturedBeams = SamusBeamCombination.Power;
        Vacate();
        Cursor = null;
        FrameHold = WrappingCountdown.Start(0);
        ClearKindState();
        Sprite = null;
        FrameBehavior = SamusProjectileFrameBehavior.None;
    }

    /// <summary>Frees the slot: no projectile holds it, and it carries no damage.</summary>
    private void Vacate()
    {
        _damage = 0;
        IsOccupied = false;
    }

    /// <summary>Clears the state each projectile kind keeps: missile ignition and link, particle angles and speeds, the Chainsaw fuse.</summary>
    private void ClearKindState()
    {
        MissileIgnition = SamusMissileIgnition.Pending;
        SuperMissileLinkSlot = 0;
        ComboAngle = default;
        WaveComboVerticalSpeed = default;
        ComboOrbitRadius = 0;
        ComboLifetime = 0;
        PlasmaPhase = PlasmaRingPhase.Expanding;
        SpazerAngleStep = 0;
        EchoRadius = 0;
        EchoAngle = default;
        ChainsawPowerBombFuse = WrappingCountdown.Start(0);
    }
}
