namespace SuperMetroid.Core.Game;

/// <summary>Canonical action/direction bits documented by bank91 TransitionTable; combinations are required chords.</summary>
[Flags]
internal enum CanonicalPoseButtons : ushort
{
    /// <summary>$91:9EE2 TransitionTable input bits: canonical aim diagonally up.</summary>
    AimUp = 0x0010,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical aim diagonally down.</summary>
    AimDown = 0x0020,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical Shoot.</summary>
    Shoot = 0x0040,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical Jump.</summary>
    Jump = 0x0080,
    /// <summary>$91:9EE2 TransitionTable input bits: Right direction.</summary>
    Right = 0x0100,
    /// <summary>$91:9EE2 TransitionTable input bits: Left direction.</summary>
    Left = 0x0200,
    /// <summary>$91:9EE2 TransitionTable input bits: Down direction.</summary>
    Down = 0x0400,
    /// <summary>$91:9EE2 TransitionTable input bits: Up direction.</summary>
    Up = 0x0800,
}
