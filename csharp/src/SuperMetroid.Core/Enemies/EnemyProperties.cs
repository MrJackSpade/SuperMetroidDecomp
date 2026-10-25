namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Independently combinable bits in the native enemy-property word. Only meanings already
/// exercised by translated code are named; unknown cartridge bits remain raw rather than
/// receiving plausible-but-unverified labels.
/// </summary>
[Flags]
public enum EnemyProperties : ushort
{
    /// <summary>No property is set.</summary>
    None = 0,

    /// <summary>Suppresses ordinary enemy drawing while leaving the slot's AI and collision state resident.</summary>
    Invisible = 0x0100,

    /// <summary>Marks the physical slot as deleted so the room-enemy passes no longer process it.</summary>
    Deleted = 0x0200,

    /// <summary>Excludes the enemy from the ordinary touch-collision pass against Samus.</summary>
    IgnoreSamusCollision = 0x0400,

    /// <summary>Allows enemy AI and instructions to run when the actor lies outside the active screen bounds.</summary>
    ProcessOffScreen = 0x0800,

    /// <summary>Prevents Plasma Beam projectiles from passing through the enemy after a hit.</summary>
    BlocksPlasmaBeam = 0x1000,

    /// <summary>Enables execution of the enemy's timed instruction-list stream.</summary>
    ProcessInstructions = 0x2000,

    /// <summary>
    /// Native property $4000. Enemy death retains the physical slot as definition $DAFF,
    /// and the death-projectile instruction stream later reconstructs it from the immutable
    /// population/spawn record. Rinka is the first translated family that exercises this
    /// lifecycle, so the bit now has directly observed behavior rather than a speculative name.
    /// </summary>
    RespawnIfKilled = 0x4000,

    /// <summary>
    /// Makes the actor participate in the solid-enemy collision pass even while unfrozen.
    /// Frozen enemies enter that pass independently of this flag.
    /// </summary>
    SolidToSamus = 0x8000,
}
