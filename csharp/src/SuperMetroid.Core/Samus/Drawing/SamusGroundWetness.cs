namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>When the ground Samus stands on is wet, so that her running footsteps and her landings splash (#2416).</summary>
internal enum SamusGroundWetness
{
    /// <summary>Dry: her footsteps raise dust only while she speed-boosts, and her landings raise what the ground's dry landings do.</summary>
    Dry,
    /// <summary>Always wet.</summary>
    Wet,
    /// <summary>Wet while it rains, as on the Landing Site.</summary>
    WetWhileRaining,
    /// <summary>Wet at and below the Wrecked Ship entrance's waterline.</summary>
    WetBelowWaterline,
}
