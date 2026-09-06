namespace SuperMetroid.Core.Game;

/// <summary>Exclusive values of samus_x_accel_mode at WRAM $0B4A, consumed by bank $90.</summary>
public static class SamusHorizontalAccelerationModes
{
    /// <summary>$0B4A = 0: ordinary forward acceleration; selected by $91:ECD0 and no-dash aerial initialization.</summary>
    public const ushort Accelerating = 0;

    /// <summary>$0B4A = 1: decelerate while retaining the old world direction during a facing reversal.</summary>
    public const ushort Reversing = 1;

    /// <summary>$0B4A = 2: retained-momentum deceleration; selected by $91:F543/$91:F60D when extra dash speed remains.</summary>
    public const ushort Decelerating = 2;
}
