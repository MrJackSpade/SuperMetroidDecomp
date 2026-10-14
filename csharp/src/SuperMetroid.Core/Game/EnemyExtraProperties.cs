namespace SuperMetroid.Core.Game;

/// <summary>
/// Independently combinable bits in the native enemy extra-property word whose behavior is
/// already proven by the live room-enemy path.
/// </summary>
[Flags]
public enum EnemyExtraProperties : ushort
{
    /// <summary>No translated extra-property behavior is enabled.</summary>
    None = 0,

    /// <summary>
    /// Selects the extended-spritemap path and admits the actor even when its ordinary
    /// radius-based visibility test would reject it.
    /// </summary>
    UsesExtendedSpritemap = 0x0004,

    /// <summary>Set when the instruction interpreter installs a new timed frame.</summary>
    NewInstructionFrame = 0x8000,
}
