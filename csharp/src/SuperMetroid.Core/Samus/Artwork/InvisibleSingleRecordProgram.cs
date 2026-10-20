namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The standalone invisible programs whose single record selects the empty composition, by bank-$93 address.</summary>
internal enum InvisibleSingleRecordProgram : ushort
{
    /// <summary>$93:873B: upward Wave's invisible lead-in before the shared vertical-travel oscillation.</summary>
    WaveLeadIn = 0x873b,
    /// <summary>$93:8F17: invisible upward charged-Wave lead-in.</summary>
    ChargedWaveLeadIn = 0x8f17,
    /// <summary>$93:9153: invisible upward charged IceWave lead-in.</summary>
    ChargedIceWaveLeadIn = 0x9153,
    /// <summary>$93:9F7B: the super-missile link has no visible composition.</summary>
    SuperMissileLink = 0x9f7b,
}
