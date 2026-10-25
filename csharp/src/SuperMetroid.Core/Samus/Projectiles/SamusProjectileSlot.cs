using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One debugger-readable ordinary slot corresponding to a native even byte index.</summary>
public sealed class SamusProjectileSlot
{
    /// <summary>Associates a host slot with the native parallel-array byte index twice its ordinal.</summary>
    internal SamusProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Host slot 0..4; native byte index is this value times two.</summary>
    [IntegerQuantity("position of the slot in the projectile table")]
    public int SlotIndex { get; }

    /// <summary>Native byte index used by the parallel WRAM arrays.</summary>
    public int SlotTableOffset => SlotIndex * 2;

    /// <summary>Damage doubles as the producer's free-slot sentinel.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Projectile family/beam flags from WRAM <c>$0C18</c>.</summary>
    [PackedFields("WRAM $0C18 projectile type word; PackedType decodes its family and beam fields, and zero marks an empty slot")]
    public int Type { get; internal set; }

    /// <summary>
    /// Lossless semantic view of <see cref="Type"/>. Raw storage remains public for exact
    /// WRAM comparison and debugging; consumers no longer need to repeat packed masks.
    /// </summary>
    public SamusProjectileTypeWord PackedType => new(checked((ushort)Type));

    /// <summary>Low nibble is direction 0..9; upper bits are lifecycle flags.</summary>
    public int Direction { get; internal set; }

    /// <summary>Lossless semantic view of <see cref="Direction"/>.</summary>
    public SamusProjectileDirectionWord PackedDirection => new(checked((ushort)Direction));

    /// <summary>Room-pixel 16.16 position: whole words <c>$0B64/$0B78</c>, subpixels <c>$0B8C/$0BA0</c>.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>
    /// Signed 8.8 per-frame velocity words <c>$0C2C/$0C40</c>. Charge combos reuse the same two
    /// WRAM words for their own orbit radius, lifetime and phase counters.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Collision half-extents in pixels.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>Where the slot's projectile program resumes; null marks the slot inactive for animation.</summary>
    internal AnimationProgramCursor<SamusProjectileProgram>? Cursor { get; set; }
    /// <summary>Updates remaining before the current projectile instruction advances.</summary>
    public WrappingCountdown InstructionTimer { get; internal set; }
    /// <summary>The composition the projectile shows (native <c>$0CC2 + 2*Index</c>), selected by the timed-frame interpreter or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Current animation frame index.</summary>
    public int AnimationFrame
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Countdown controlling projectile-trail emission.</summary>
    public WrappingCountdown TrailTimer { get; internal set; }
    /// <summary>WRAM <c>$0C7C</c>; missile ignition/acceleration state in the high byte.</summary>
    [PackedFields("projectile variable word whose meaning each projectile family decodes")]
    public int Variable
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>WRAM $0CA4: phase word used by Spazer special-attack particles.</summary>
    public SpazerComboPhase AuxiliaryPhase { get; internal set; }
    /// <summary>Semantic identity of the slot's bank-$90 pre-instruction.</summary>
    public SamusProjectilePreInstruction PreInstruction { get; internal set; }

    /// <summary>Bank-$93 considers a nonzero instruction pointer allocated and drawable.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>
    /// Bank-$A0 enemy collision admits a nonzero beam, missile, or Super Missile type even
    /// when bank $93 has no instruction list to animate. This distinction is observable for
    /// the left-facing Murder Beam, whose type and damage remain live while its list is zero.
    /// </summary>
    public bool HasEnemyCollisionPayload =>
        Type != 0 &&
        PackedType.Family is not (
            SamusProjectileFamily.PowerBomb or
            SamusProjectileFamily.Bomb) &&
        !PackedType.IsFamilyAtOrAfter(SamusProjectileFamily.BeamExplosion);

    /// <summary>Clears the words owned by $90:ADB7; trail and auxiliary words survive reuse.</summary>
    internal void ClearFields()
    {
        Damage = 0;
        Type = 0;
        Direction = 0;
        Position = FixedPosition<RoomPixels>.Zero;
        Velocity = default(EightEightVector<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Cursor = null;
        InstructionTimer = WrappingCountdown.Start(0);
        Sprite = null;
        AnimationFrame = 0;
        Variable = 0;
        PreInstruction = SamusProjectilePreInstruction.None;
    }

    /// <summary>
    /// The per-slot words <c>ResetProjectileData</c> ($90:AD22) clears on a door load or
    /// elevator ride. Unlike deletion it keeps both subpixels, the animation frame and
    /// $0CA4, which a later shot in the slot inherits.
    /// </summary>
    internal void ClearForProjectileDataReset()
    {
        TrailTimer = WrappingCountdown.Start(0);
        Position = Position.WithWhole(Position16<RoomPixels>.Zero);
        Direction = 0;
        Velocity = default(EightEightVector<RoomPixels>);
        Radius = Extent16<RoomPixels>.Zero;
        Type = 0;
        Damage = 0;
        Cursor = null;
        InstructionTimer = WrappingCountdown.Start(0);
        Variable = 0;
        Sprite = null;
        PreInstruction = SamusProjectilePreInstruction.None;
    }
}
