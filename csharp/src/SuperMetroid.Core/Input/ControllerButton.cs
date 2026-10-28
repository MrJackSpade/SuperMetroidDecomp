namespace SuperMetroid.Core.Input;

/// <summary>
/// The player's controller buttons. Each button is one independent bit, and a set of held buttons is one flags value;
/// the bit order is the one recorded inputs, demo programs and saved bindings store.
/// </summary>
[Flags]
public enum ControllerButton
{
    /// <summary>No controller buttons are held.</summary>
    None = 0,

    /// <summary>The right shoulder button.</summary>
    R = 1 << 4,

    /// <summary>The left shoulder button.</summary>
    L = 1 << 5,

    /// <summary>The upper face button.</summary>
    X = 1 << 6,

    /// <summary>The right face button.</summary>
    A = 1 << 7,

    /// <summary>The directional pad's right direction.</summary>
    Right = 1 << 8,

    /// <summary>The directional pad's left direction.</summary>
    Left = 1 << 9,

    /// <summary>The directional pad's down direction.</summary>
    Down = 1 << 10,

    /// <summary>The directional pad's up direction.</summary>
    Up = 1 << 11,

    /// <summary>The Start button.</summary>
    Start = 1 << 12,

    /// <summary>The Select button.</summary>
    Select = 1 << 13,

    /// <summary>The left face button.</summary>
    Y = 1 << 14,

    /// <summary>The lower face button.</summary>
    B = 1 << 15,
}
