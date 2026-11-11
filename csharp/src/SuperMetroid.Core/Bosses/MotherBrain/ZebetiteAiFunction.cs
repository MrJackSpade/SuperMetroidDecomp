namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The three Zebetite AI functions, in order.</summary>
public enum ZebetiteAiFunction
{
    /// <summary>Primary setup allocates a linked half when the generation flag is negative, links slot indices, then enters the door wait in the same update.</summary>
    SpawnLinkedHalf = 1,
    /// <summary>Remains inert while the shared door-transition flag is set, then installs and executes the active function immediately.</summary>
    WaitForDoorTransition,
    /// <summary>Updates palette/health artwork, regenerates one health per call up to 1000, or handles death with primary-owned generation progression.</summary>
    Active,
}
