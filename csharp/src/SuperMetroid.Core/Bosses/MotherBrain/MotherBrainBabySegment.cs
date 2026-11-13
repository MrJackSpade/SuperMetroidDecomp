namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cutscene Baby Metroid's programs, order.</summary>
public enum MotherBrainBabySegment
{
    /// <summary>The slow idle pulse.</summary>
    Initial,
    /// <summary>The quick draining pulse.</summary>
    DrainingMotherBrain,
    /// <summary>Baby Metroid animation script: taking fatal blow.</summary>
    TakingFatalBlow,
}
