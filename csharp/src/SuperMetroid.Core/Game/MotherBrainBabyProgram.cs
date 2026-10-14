namespace SuperMetroid.Core.Game;

/// <summary>The cutscene Baby Metroid's programs, in bank $A9 order.</summary>
public enum MotherBrainBabyProgram
{
    /// <summary><c>InstList_BabyMetroid_Initial</c> (<c>$A9:CFA2</c>): the slow idle pulse.</summary>
    Initial,
    /// <summary><c>InstList_BabyMetroid_DrainingMotherBrain</c> (<c>$A9:CFB8</c>): the quick draining pulse.</summary>
    DrainingMotherBrain,
    /// <summary><c>InstList_BabyMetroid_TakingFatalBlow</c> (<c>$A9:CFCE</c>).</summary>
    TakingFatalBlow,
}
