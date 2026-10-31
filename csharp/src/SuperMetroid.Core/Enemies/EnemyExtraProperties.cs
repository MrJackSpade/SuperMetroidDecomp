namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy's additional engine properties: how it draws and whether its animation just installed a frame. The
/// cartridge treats them as independently composable bits.
/// </summary>
[Flags]
public enum EnemyExtraProperties
{
    /// <summary>No additional property is set.</summary>
    None = 0,

    /// <summary>
    /// Selects the extended-spritemap path and admits the actor even when its ordinary
    /// radius-based visibility test would reject it.
    /// </summary>
    UsesExtendedSpritemap = 1 << 2,

    /// <summary>Set when the instruction interpreter installs a new timed frame.</summary>
    NewInstructionFrame = 1 << 15,
}
