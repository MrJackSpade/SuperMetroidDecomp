namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres Baby Metroid's cutscene programs, in authored order.</summary>
internal enum CeresBabyProgram
{
    /// <summary><c>InstList_BabyMetroidCutscene_0</c> at $A6:BF31: the idle squish, two squish cycles that each first check whether the baby moves.</summary>
    Initial,
    /// <summary><c>InstList_BabyMetroidCutscene_1</c> at $A6:BF59: the crying squish while it moves, a cry and then twelve poses whose palette pulses with them, slowing back down.</summary>
    ExpressiveLoop,
}
