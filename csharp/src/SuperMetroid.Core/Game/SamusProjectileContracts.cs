namespace SuperMetroid.Core.Game;

/// <summary>One debugger-readable ordinary slot corresponding to a native even byte index.</summary>
public sealed class SamusProjectileSlot
{
    internal SamusProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Host slot 0..4; native byte index is this value times two.</summary>
    public int SlotIndex { get; }

    /// <summary>Native byte index used by the parallel WRAM arrays.</summary>
    public int NativeByteIndex => SlotIndex * 2;

    /// <summary>Damage doubles as the producer's free-slot sentinel.</summary>
    public ushort Damage { get; internal set; }

    /// <summary>Projectile family/beam flags from WRAM <c>$0C18</c>.</summary>
    public ushort Type { get; internal set; }

    /// <summary>
    /// Lossless semantic view of <see cref="Type"/>. Raw storage remains public for exact
    /// WRAM comparison and debugging; consumers no longer need to repeat packed masks.
    /// </summary>
    public SamusProjectileTypeWord PackedType => new(Type);

    /// <summary>Low nibble is direction 0..9; upper bits are lifecycle flags.</summary>
    public ushort Direction { get; internal set; }

    /// <summary>Lossless semantic view of <see cref="Direction"/>.</summary>
    public SamusProjectileDirectionWord PackedDirection => new(Direction);

    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public short XVelocity { get; internal set; }
    public short YVelocity { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort AnimationFrame { get; internal set; }
    public ushort TrailTimer { get; internal set; }
    /// <summary>WRAM <c>$0C7C</c>; missile ignition/acceleration state in the high byte.</summary>
    public ushort Variable { get; internal set; }
    public SamusProjectilePreInstruction PreInstruction { get; internal set; }

    /// <summary>Bank-$93 considers a nonzero instruction pointer allocated and drawable.</summary>
    public bool IsActive => InstructionPointer != 0;

    internal void ClearFields()
    {
        Damage = 0;
        Type = 0;
        Direction = 0;
        XPosition = 0;
        YPosition = 0;
        XSubposition = 0;
        YSubposition = 0;
        XVelocity = 0;
        YVelocity = 0;
        XRadius = 0;
        YRadius = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        SpritemapPointer = 0;
        AnimationFrame = 0;
        TrailTimer = 0;
        Variable = 0;
        PreInstruction = SamusProjectilePreInstruction.None;
    }
}

/// <summary>Semantic identities for the bank-$90 function pointers stored per slot.</summary>
public enum SamusProjectilePreInstruction : byte
{
    None,
    NoWaveBeam,
    WaveBeamThreeFrameTrail,
    WaveBeamFourFrameTrail,
    HyperBeam,
    Missile,
    SuperMissile,
    SuperMissileLink,
}

/// <summary>
/// One of the eighteen independent projectile-trail allocations. The left timer is the
/// native free-slot sentinel even though both sides otherwise animate independently.
/// </summary>
public sealed class SamusProjectileTrailSlot
{
    internal SamusProjectileTrailSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    public int SlotIndex { get; }
    public int NativeByteIndex => SlotIndex * 2;
    public SamusProjectileTrailSide Left { get; }
    public SamusProjectileTrailSide Right { get; }
    public bool IsActive => Left.InstructionTimer != 0;

    internal void ClearFields()
    {
        Left.ClearFields();
        Right.ClearFields();
    }
}

/// <summary>One side of a two-stream bank-$90 projectile-trail animation.</summary>
public sealed class SamusProjectileTrailSide
{
    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort TileNumberAttributes { get; internal set; }

    internal void ClearFields()
    {
        XPosition = 0;
        YPosition = 0;
        InstructionTimer = 0;
        InstructionPointer = 0;
        TileNumberAttributes = 0;
    }
}

/// <summary>Immutable summary of one ordinary-projectile alpha pass.</summary>
public readonly record struct SamusProjectileFrameResult(
    int? FiredSlot,
    ushort QueuedSoundEffect,
    bool CollisionStartedExplosion,
    bool ProjectileDeleted);

/// <summary>
/// Immutable producer-phase evidence captured before the new projectile's first movement.
/// A beam may collide or leave the native movement window in that same alpha pass, so the
/// mutable slot can already be clear by the time a debugger inspects the frame result.
/// </summary>
public readonly record struct SamusProjectileSpawnSnapshot(
    int SlotIndex,
    ushort Direction,
    ushort XPosition,
    ushort YPosition,
    short XVelocity,
    short YVelocity);

/// <summary>Semantic branch and raw table/timer evidence from one `$91:D743` call.</summary>
public readonly record struct SamusBeamChargePaletteStepResult(
    SamusBeamChargePaletteAction Action,
    ushort TimerBefore,
    ushort TimerAfter,
    ushort PalettePointer,
    int? HyperPaletteIndex,
    int? ChargePaletteIndex = null);

/// <summary>Named outcomes of the nonzero charged-shot glow branches.</summary>
public enum SamusBeamChargePaletteAction : byte
{
    Inactive,
    ChargeCycle,
    PseudoScrewCycle,
    OrdinaryWhite,
    HyperPalette,
    HyperHold,
    RestoredNormalSuit,
}
