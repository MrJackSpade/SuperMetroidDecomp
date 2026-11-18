namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Reads where the shine's glow stands as the step the original counts it by (#2387).</summary>
internal static class SamusShinesparkGlowAccess
{
    /// <summary>Reads the shine's glow.</summary>
    extension(SamusShinesparkState self)
    {
        /// <summary>The shade the shine's glow shows next, counted from its first: zero at its start and after its last.</summary>
        internal int ShadeStep => self.ShineGlow.GoingOnIn(self.ShineGlow.Animation).Place.Beat;
    }
}
