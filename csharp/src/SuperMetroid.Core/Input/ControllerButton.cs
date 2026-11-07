namespace SuperMetroid.Core.Input;

/// <summary>
/// The player's controller buttons. Each button is one independent bit, and a set of held buttons is one flags value.
/// The bit values are arbitrary: nothing reads a button's number, and stored forms convert through their own codec.
/// </summary>
[Flags]
public enum ControllerButton
{
    /// <summary>No controller buttons are held.</summary>
    None = 0,

    /// <summary>The directional pad's up direction.</summary>
    Up = 1 << 0,

    /// <summary>The directional pad's down direction.</summary>
    Down = 1 << 1,

    /// <summary>The directional pad's left direction.</summary>
    Left = 1 << 2,

    /// <summary>The directional pad's right direction.</summary>
    Right = 1 << 3,

    /// <summary>The Start button.</summary>
    Start = 1 << 4,

    /// <summary>The Select button.</summary>
    Select = 1 << 5,

    /// <summary>The right face button.</summary>
    A = 1 << 6,

    /// <summary>The lower face button.</summary>
    B = 1 << 7,

    /// <summary>The upper face button.</summary>
    X = 1 << 8,

    /// <summary>The left face button.</summary>
    Y = 1 << 9,

    /// <summary>The left shoulder button.</summary>
    L = 1 << 10,

    /// <summary>The right shoulder button.</summary>
    R = 1 << 11,
}
