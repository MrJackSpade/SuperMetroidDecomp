namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads the shade the Hyper Beam's colour effect last showed, which only the effect keeps (#2350).</summary>
internal static class HyperBeamColourEffectStateAccess
{
    /// <summary>Reads the effect's own state.</summary>
    extension(HyperBeamColourEffectState self)
    {
        /// <summary>The shade of the beams' cycle last shown, by its place in the cycle, or none before the first update.</summary>
        internal int? ShownCycleShade => PrivateState.Field<HyperBeamColourEffectState, int?>(self, "_shownCycleShade");
    }
}
