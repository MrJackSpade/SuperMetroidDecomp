namespace SuperMetroid.Core.Game;

/// <summary>Atmospheric-slot writes made by $91:FA76, SamusFunc_F468_WallJumping.</summary>
internal static class SamusWallJumpDustDefinitions
{
    /// <summary>$91:FA76 writes only atmospheric slot three.</summary>
    public const int Slot = 3;
    /// <summary>$91:FA76 installs packed type $0600, the shared dry dust animation.</summary>
    public const byte Type = 6;
    /// <summary>$91:FA76 starts the dust at frame zero with three animation ticks.</summary>
    public const ushort InitialTimer = 3;
    /// <summary>$91:FA76 places dust six pixels behind the new pose's facing direction.</summary>
    public const int HorizontalOffset = 6;
}
