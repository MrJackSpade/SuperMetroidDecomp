namespace SuperMetroid.Core.Game;

/// <summary>One semantic view over an enemy-projectile WRAM slot.</summary>
public sealed class MotherBrainEnemyProjectileSlot
{
    internal MotherBrainEnemyProjectileSlot(int index) => Index = index;

    public int Index { get; }
    public ushort ProjectileId { get; internal set; }

    /// <summary>
    /// Definition property word copied from bank $86. Bit $1000 selects the native
    /// high-priority draw pass; the remaining bits are retained so later translations do
    /// not have to reconstruct definition state from the host projectile type.
    /// </summary>
    public ushort Properties { get; internal set; }

    public ushort GraphicsIndex { get; internal set; }

    /// <summary>Initialization parameter retained for inspecting table-selected fragments.</summary>
    public ushort SpawnParameter { get; internal set; }

    /// <summary>Blue-ring head-follow delay; zero for the other translated definitions.</summary>
    public ushort DelayTimer { get; internal set; }

    /// <summary>Door-fragment Var0 countdown; unused by blue rings, bombs, and the subtitle.</summary>
    public ushort Lifetime { get; internal set; }

    /// <summary>
    /// Mother Brain bomb Var0: the absolute horizontal speed restored at each floor bounce.
    /// </summary>
    public ushort BounceHorizontalSpeed { get; internal set; }

    /// <summary>
    /// Mother Brain bomb Var1: an even byte offset into `$86:C550`'s acceleration table.
    /// Zero has the special pre-first-bounce friction path; `$12` selects the terminating zero.
    /// </summary>
    public ushort BounceTableOffset { get; internal set; }
    public SnesAngle Angle { get; internal set; }
    public ushort XPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public ushort XVelocity { get; internal set; }
    public ushort YVelocity { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }

    public bool IsActive => ProjectileId != 0;

    internal void Clear()
    {
        ProjectileId = 0;
        Properties = 0;
        GraphicsIndex = 0;
        SpawnParameter = 0;
        DelayTimer = 0;
        Lifetime = 0;
        BounceHorizontalSpeed = 0;
        BounceTableOffset = 0;
        Angle = SnesAngle.Zero;
        XPosition = 0;
        XSubposition = 0;
        YPosition = 0;
        YSubposition = 0;
        XVelocity = 0;
        YVelocity = 0;
        XRadius = 0;
        YRadius = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        SpritemapPointer = 0;
    }
}

/// <summary>Collision/deletion reason produced by one ring pre-instruction.</summary>
public enum MotherBrainOnionRingCollisionKind
{
    None,
    BabyMetroid,
    Samus,
    RoomBoundary,
    DeletedAfterBabyDeath,
}

/// <summary>Debugger witness for one ring collision.</summary>
public readonly record struct MotherBrainOnionRingEvent(
    int SlotIndex,
    MotherBrainOnionRingCollisionKind Collision,
    ushort XPosition,
    ushort YPosition,
    ushort TargetHealthBefore,
    ushort TargetHealthAfter);

/// <summary>Aggregate result of one native enemy-projectile pass.</summary>
public readonly record struct MotherBrainEnemyProjectileFrameResult(
    int ActiveCount,
    IReadOnlyList<MotherBrainOnionRingEvent> Events,
    IReadOnlyList<MotherBrainEscapeDoorParticleDustRequest> EscapeDoorDustRequests,
    IReadOnlyList<MotherBrainBombEvent> BombEvents);

/// <summary>Observable transition emitted by `$86:C4C8-C604`'s bomb pre-instruction.</summary>
public enum MotherBrainBombEventKind
{
    Bounced,
    DestroyedBySamusBomb,
    Expired,
}

/// <summary>
/// One debugger-visible Mother Brain bomb bounce or deletion and its external spawn/sound
/// requests. A normal movement call intentionally emits no event; its exact state remains on
/// the projectile slot for stepping and watch windows.
/// </summary>
public readonly record struct MotherBrainBombEvent(
    int SlotIndex,
    MotherBrainBombEventKind Kind,
    ushort XPosition,
    ushort YPosition,
    ushort BounceTableOffset,
    ushort? AfterburnCount,
    ushort DustParameter,
    bool EnemyDropRequested,
    ushort? QueuedSoundLibraryThree);

/// <summary>
/// Final parameter-nine misc-dust spawn produced when one `$86:CB21` fragment expires.
/// </summary>
public readonly record struct MotherBrainEscapeDoorParticleDustRequest(
    int SourceSlotIndex,
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter);
