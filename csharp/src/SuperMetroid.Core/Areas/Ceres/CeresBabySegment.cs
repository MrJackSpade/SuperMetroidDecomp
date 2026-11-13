namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres Baby Metroid's cutscene programs, in authored order.</summary>
internal enum CeresBabySegment
{
    /// <summary>The idle squish, two squish cycles that each first check whether the baby moves.</summary>
    Initial,
    /// <summary>The crying squish while it moves, a cry and then twelve poses whose palette pulses with them, slowing back down.</summary>
    ExpressiveLoop,
}
