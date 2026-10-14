namespace SuperMetroid.Core.Game;

/// <summary>The Tourian Shitroid's programs, in bank $A9 order.</summary>
public enum ShitroidProgram
{
    /// <summary><c>InstList_BabyMetroid_FinishDraining</c> (<c>$A9:F906</c>): a long wide-open hold, then calm.</summary>
    FinishDraining,
    /// <summary><c>InstList_BabyMetroid_Normal</c> (<c>$A9:F90E</c>): the calm pulse.</summary>
    Normal,
    /// <summary><c>InstList_BabyMetroid_LatchedOn</c> (<c>$A9:F924</c>): the quick draining pulse.</summary>
    LatchedOn,
    /// <summary><c>InstList_BabyMetroid_Remorse</c> (<c>$A9:F93A</c>): the hesitant pulse before leaving.</summary>
    Remorse,
}
