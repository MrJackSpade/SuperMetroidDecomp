namespace SuperMetroid.Core.Game;

/// <summary>
/// Drop request emitted when a shot destroys Magdollite's thrown lava. The shared pickup
/// system still owns random drop selection; this record preserves the exact source enemy
/// definition and bank-$86 projectile slot used by instruction $DFEA.
/// </summary>
public readonly record struct MagdolliteLavaDropRequest();
