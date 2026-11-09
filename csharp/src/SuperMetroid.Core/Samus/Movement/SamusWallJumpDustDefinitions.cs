using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The dust a wall jump kicks up as it starts.</summary>
internal static class SamusWallJumpDustDefinitions
{
    /// <summary>The wall jump kicks up the dust a landing raises.</summary>
    public const SamusParticleKind Kind = SamusParticleKind.Dust;
    /// <summary>The dust shows its first step for three updates.</summary>
    public const int FirstStepUpdates = 3;
    /// <summary>Places dust six pixels behind the new pose's facing direction.</summary>
    public const int HorizontalOffset = 6;
}
