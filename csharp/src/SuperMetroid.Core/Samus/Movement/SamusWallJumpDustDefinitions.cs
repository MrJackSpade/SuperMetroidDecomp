using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Atmospheric-slot writes made when a wall jump starts.</summary>
internal static class SamusWallJumpDustDefinitions
{
    /// <summary>Writes only atmospheric slot three.</summary>
    public const int Slot = 3;
    /// <summary>Installs packed type 1536, the shared dry dust animation.</summary>
    public const SamusAtmosphericEffectType Type = SamusAtmosphericEffectType.Dust;
    /// <summary>Starts the dust at frame zero with three animation ticks.</summary>
    public const int InitialTimer = 3;
    /// <summary>Places dust six pixels behind the new pose's facing direction.</summary>
    public const int HorizontalOffset = 6;
}
