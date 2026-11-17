namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>What the landing site's lightning watches while it runs.</summary>
internal static class CrateriaLightningDefinitions
{
    /// <summary>
    /// The room height Samus must stand below for the landing site's lightning to play on: while she stands higher, it
    /// restarts its calm (<see cref="ColourEffectCondition.RestartsWhileSamusIsHigh"/>).
    /// </summary>
    public const int SamusHighOnSurfaceHeight = 896;
}
