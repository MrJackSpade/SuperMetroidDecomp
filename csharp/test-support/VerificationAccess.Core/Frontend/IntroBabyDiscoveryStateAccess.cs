using SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="IntroBabyDiscoveryState"/> members production does not use.</summary>
internal static class IntroBabyDiscoveryStateAccess
{
    /// <summary>Counts active egg particles held by the discovery scene.</summary>
    extension(IntroBabyDiscoveryState self)
    {
        /// <summary>Number of egg particles whose sprite is still active.</summary>
        internal int ActiveEggParticleCount => self.eggParticles.Count(particle => particle.IsActive);
    }
}
