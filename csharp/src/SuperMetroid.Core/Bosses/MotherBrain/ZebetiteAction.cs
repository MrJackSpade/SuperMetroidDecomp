namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The three actions a Zebetite performs.</summary>
public enum ZebetiteAction
{
    /// <summary>Primary setup allocates a linked half when the generation flag is negative, links slot indices, then enters the door wait in the same update.</summary>
    SpawnLinkedHalf = 1,
    /// <summary>Remains inert while the shared door-transition flag is set, then installs and performs the active action immediately.</summary>
    WaitForDoorTransition,
    /// <summary>Updates palette/health artwork, regenerates one health per call up to 1000, or handles death with primary-owned generation progression.</summary>
    Active,
}
