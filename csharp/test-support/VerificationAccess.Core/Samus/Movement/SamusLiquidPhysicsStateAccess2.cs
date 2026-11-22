namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Reads Samus's water surface as her liquid physics hold it.</summary>
internal static class SamusLiquidPhysicsStateAccess2
{
    /// <summary>Reads the surface.</summary>
    extension(SamusLiquidPhysicsState self)
    {
        /// <summary>Whether Samus's movement ignores the room's water surface until a room object enables it.</summary>
        internal bool IgnoresWaterSurface => self.Surface is SamusLiquidSurface.Water { IgnoredUntilEnabled: true };
    }
}
