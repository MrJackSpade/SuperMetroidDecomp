using System.Reflection;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Verification access to the particles Samus's presentation shows (#1556). The presentation only advances and draws
/// them; checks read the list to assert each particle's kind, step and countdown.
/// </summary>
internal static class SamusParticlesAccess
{
    /// <summary>Exposes the particle list.</summary>
    extension(SamusParticles self)
    {
        /// <summary>The particles shown or waiting to show, oldest first.</summary>
        internal IReadOnlyList<SamusParticle> All =>
            typeof(SamusParticles).GetField("_particles", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(self) as List<SamusParticle>
                ?? throw new InvalidOperationException("SamusParticles no longer keeps its particles in _particles.");
    }
}
