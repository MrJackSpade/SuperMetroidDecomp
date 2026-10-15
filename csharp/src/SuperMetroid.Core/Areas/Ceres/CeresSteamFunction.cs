namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Mutually exclusive bank-$A6 main-function identities stored in the steam actor's
/// variable A by <c>$A6:EFB1</c> and dispatched by <c>$A6:F00D</c>.
/// </summary>
public enum CeresSteamFunction : ushort
{
    /// <summary>The shared return at <c>$A6:EFF4</c>; no graphical offset is required.</summary>
    None = 0xeff4,

    /// <summary>Applies the rotating-elevator graphical transform at <c>$A6:F019</c>.</summary>
    ApplyRotatingElevatorOffset = 0xf019,
}
