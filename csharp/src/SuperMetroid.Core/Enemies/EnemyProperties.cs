namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The engine properties of an enemy, which its population entry starts and its behaviour toggles: how the common
/// passes draw, process and collide with it. The cartridge treats them as independently composable bits.
/// </summary>
[Flags]
public enum EnemyProperties
{
    /// <summary>No property is set.</summary>
    None = 0,

    /// <summary>Suppresses ordinary enemy drawing while leaving the slot's AI and collision state resident.</summary>
    Invisible = 1 << 8,

    /// <summary>Marks the physical slot as deleted so the room-enemy passes no longer process it.</summary>
    Deleted = 1 << 9,

    /// <summary>Excludes the enemy from the ordinary touch-collision pass against Samus.</summary>
    IgnoreSamusCollision = 1 << 10,

    /// <summary>Allows enemy AI and instructions to run when the actor lies outside the active screen bounds.</summary>
    ProcessOffScreen = 1 << 11,

    /// <summary>Prevents Plasma Beam projectiles from passing through the enemy after a hit.</summary>
    BlocksPlasmaBeam = 1 << 12,

    /// <summary>Enables execution of the enemy's timed instruction-list stream.</summary>
    ProcessInstructions = 1 << 13,

    /// <summary>
    /// Enemy death keeps the slot occupied by the respawn placeholder, and the death-projectile instruction stream later
    /// rebuilds the enemy from its unchanged population entry. Rinka is the family that exercises this lifecycle.
    /// </summary>
    RespawnIfKilled = 1 << 14,

    /// <summary>
    /// Makes the actor participate in the solid-enemy collision pass even while unfrozen.
    /// Frozen enemies enter that pass independently of this flag.
    /// </summary>
    SolidToSamus = 1 << 15,
}
