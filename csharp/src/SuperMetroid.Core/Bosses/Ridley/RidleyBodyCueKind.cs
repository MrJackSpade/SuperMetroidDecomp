namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What a cue of Ridley's body programs does: one per bank-$A6 Ridley instruction its lists use.</summary>
internal enum RidleyBodyCueKind
{
    /// <summary>Raises the roaring flag and queues the roar (<c>Instruction_Ridley_Roar</c>, $A6:E4BE).</summary>
    Roar,
    /// <summary>Clears the roaring flag (<c>Instruction_Ridley_ClearRoaringFlag</c>, $A6:E4CA).</summary>
    ClearRoaringFlag,
    /// <summary>Faces forward while turning (<c>Instruction_Ridley_FaceRidleyForward</c>, $A6:E727).</summary>
    FaceForward,
    /// <summary>Faces left and mirrors the tail (<c>Instruction_Ridley_FlipRidleyLeft</c>, $A6:E71C).</summary>
    FlipLeft,
    /// <summary>Faces right and mirrors the tail (<c>Instruction_Ridley_FlipRidleyRight</c>, $A6:E72F).</summary>
    FlipRight,
    /// <summary>Aims the next fireball at Samus (<c>Instruction_Ridley_CalculateFireballXYVelocities</c>, $A6:E84D).</summary>
    AimFireball,
    /// <summary>Spawns a fireball with its afterburn (<c>Instruction_Ridley_SpawnRidleysFireballWithAfterburn</c>, $A6:E904).</summary>
    SpawnFireballWithAfterburn,
    /// <summary>Spawns a fireball without an afterburn (<c>Instruction_Ridley_SpawnRidleysFireballWithoutAfterburn</c>, $A6:E909).</summary>
    SpawnFireballWithoutAfterburn,
    /// <summary>Starts Ceres Ridley's lift-off (<c>Instruction_RidleyCeres_StartLiftoff</c>, $A6:E969).</summary>
    CeresStartLiftoff,
    /// <summary>Starts Lower Norfair Ridley's lift-off into the fight (<c>Instruction_Ridley_StartLiftoff</c>, $A6:E976).</summary>
    StartLiftoff,
}
