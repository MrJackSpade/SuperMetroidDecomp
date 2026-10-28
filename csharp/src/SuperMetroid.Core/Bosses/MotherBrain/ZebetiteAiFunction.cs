namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The three Zebetite AI functions stored in variable A, in bank $A6 order.</summary>
public enum ZebetiteAiFunction
{
    /// <summary>$A6:FC41, Function_Zebetite_SpawnBottomZebetiteIfNeeded: primary setup allocates a linked half when the generation flag is negative, links native slot indices, then enters the door wait in the same update.</summary>
    SpawnLinkedHalf = 1,
    /// <summary>$A6:FC5B, Function_Zebetite_WaitForDoorTransitionToFinish: remains inert while shared WRAM $0795 is nonzero, then installs and executes the active function immediately.</summary>
    WaitForDoorTransition,
    /// <summary>$A6:FC67, Function_Zebetite_Active: updates palette/health artwork, regenerates one health per call up to 1000, or handles death with primary-owned generation progression.</summary>
    Active,
}
