namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The ceiling-tube pre-instruction states stored in a top tube projectile's variable zero, in bank $86 order.</summary>
internal enum MotherBrainTopTubeFunction : ushort
{
    /// <summary>$86:CBEA: puffs dust where the tube breaks away, then falls.</summary>
    SpawnDust = 1,
    /// <summary>$86:CC08: falls until it lands.</summary>
    Fall,
}
