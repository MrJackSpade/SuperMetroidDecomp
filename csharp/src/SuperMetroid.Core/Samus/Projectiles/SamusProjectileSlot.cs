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

    /// <summary>Damage doubles as the producer's free-slot sentinel.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>What the slot holds; <see cref="SamusProjectileType.None"/> marks an empty slot.</summary>
    public SamusProjectileType Type { get; internal set; }

    /// <summary>The direction the projectile was fired or reflected in.</summary>
    public SamusProjectileDirection Direction { get; internal set; }

    /// <summary>
    /// Whether the projectile has already struck something: enemy and enemy-projectile collision set it so later
    /// collision walkers skip the projectile, and the projectile's own update then deletes or converts it.
    /// </summary>
    public bool IsSpent { get; internal set; }

    /// <summary>Room-pixel 16.16 position.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>
    /// Signed 8.8 per-update velocity. Special attacks keep their orbit radius, lifetime and phase counters here
    /// instead.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Collision half-extents in pixels.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>Where the slot's projectile program resumes; null marks the slot inactive for animation.</summary>
    internal AnimationProgramCursor<SamusProjectileProgram>? Cursor { get; set; }
    /// <summary>Updates remaining before the current projectile instruction advances.</summary>
    public WrappingCountdown InstructionTimer { get; internal set; }
    /// <summary>The composition the projectile shows, selected by the timed-frame interpreter or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Current animation frame index.</summary>
    public int AnimationFrame
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Countdown controlling projectile-trail emission.</summary>
    public WrappingCountdown TrailTimer { get; internal set; }

    /// <summary>A missile or Super Missile's ignition.</summary>
    public SamusMissileIgnition MissileIgnition { get; internal set; }

    /// <summary>
    /// The slot an ignited Super Missile moves as its invisible collision link, or null when it names none. An unlinked
    /// Super Missile names slot zero: a missile that ignites with no free slot, or is reflected before igniting, treats
    /// slot zero as its link, as the cartridge does. A reflection leaves it naming none until it re-ignites.
    /// </summary>
    public int? SuperMissileLinkSlot { get; internal set; }

    /// <summary>The orbit angle of an Ice, Plasma or Spazer special-attack particle.</summary>
    public ByteAngle ComboAngle { get; internal set; }

    /// <summary>The homing vertical speed of a Wave special-attack particle.</summary>
    public SignedEightEight WaveComboVerticalSpeed { get; internal set; }

    /// <summary>The departure angle of a Shinespark crash echo, to the angle table's whole steps.</summary>
    public ByteAngle EchoAngle { get; internal set; }

    /// <summary>The Power Bomb fuse the glitched Chainsaw beam runs over its shot.</summary>
    public WrappingCountdown ChainsawPowerBombFuse { get; internal set; }

    /// <summary>The Spazer special attack's particle phase; it survives slot reuse.</summary>
    public SpazerComboPhase AuxiliaryPhase { get; internal set; }
    /// <summary>The slot's projectile pre-instruction.</summary>
    public SamusProjectilePreInstruction PreInstruction { get; internal set; }

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
        Damage = 0;
        Type = SamusProjectileType.None;
        Direction = default;
        IsSpent = false;
        Position = FixedPosition<RoomPixels>.Zero;
        Velocity = default(EightEightVector<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Cursor = null;
        InstructionTimer = WrappingCountdown.Start(0);
        Sprite = null;
        AnimationFrame = 0;
        ClearKindState();
        PreInstruction = SamusProjectilePreInstruction.None;
    }

    /// <summary>
    /// The per-slot state a door load or elevator ride clears. Unlike deletion it keeps both subpixels, the animation
    /// frame and the Spazer phase, which a later shot in the slot inherits.
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
        Damage = 0;
        Cursor = null;
        InstructionTimer = WrappingCountdown.Start(0);
        ClearKindState();
        Sprite = null;
        PreInstruction = SamusProjectilePreInstruction.None;
    }

    /// <summary>Clears the state each projectile kind keeps: missile ignition and link, particle angles and speeds, the Chainsaw fuse.</summary>
    private void ClearKindState()
    {
        MissileIgnition = SamusMissileIgnition.Pending;
        SuperMissileLinkSlot = 0;
        ComboAngle = default;
        WaveComboVerticalSpeed = default;
        EchoAngle = default;
        ChainsawPowerBombFuse = WrappingCountdown.Start(0);
    }
}
