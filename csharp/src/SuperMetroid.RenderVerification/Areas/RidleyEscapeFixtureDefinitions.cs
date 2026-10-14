namespace SuperMetroid.RenderVerification.Areas;

/// <summary>Holds the two native Ceres Ridley hover-exit thresholds exercised by the fixture.</summary>
internal static class RidleyEscapeFixtureDefinitions
{
    /// <summary>Ceres hover AI branches to the fake retreat at 100 registered projectile contacts.</summary>
    internal const ushort HitThreshold = 100;
    /// <summary>Energy below the native 30 threshold selects the direct retreat; keep the player alive.</summary>
    internal const ushort LowEnergy = 29;
}
