namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cutscene Baby Metroid's programs, order.</summary>
public enum MotherBrainBabyProgram
{
    /// <summary><c>InstList_BabyMetroid_Initial</c>: the slow idle pulse.</summary>
    Initial,
    /// <summary><c>InstList_BabyMetroid_DrainingMotherBrain</c>: the quick draining pulse.</summary>
    DrainingMotherBrain,
    /// <summary><c>InstList_BabyMetroid_TakingFatalBlow</c>.</summary>
    TakingFatalBlow,
}
