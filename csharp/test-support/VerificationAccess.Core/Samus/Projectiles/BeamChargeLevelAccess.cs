namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads whether a beam charge is the Hyper Beam's flare.</summary>
internal static class BeamChargeLevelAccess
{
    /// <summary>Reads the level.</summary>
    extension(BeamChargeLevel self)
    {
        /// <summary>Whether this is the Hyper Beam's flare rather than a held charge.</summary>
        internal bool HyperBeamFlare => self is BeamChargeLevel.HyperBeamFlareLevel;
    }

    /// <summary>Reads the charge's level.</summary>
    extension(BeamChargeState self)
    {
        /// <summary>Whether the Hyper Beam's flare shows.</summary>
        internal bool HyperBeamFlare => self.Level is BeamChargeLevel.HyperBeamFlareLevel;
    }
}
