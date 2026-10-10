namespace SuperMetroid.Core.Game;

/// <summary>
/// The private bank-$A6 animation instructions shared by Ceres and Lower Norfair Ridley.
/// </summary>
internal enum RidleyInstruction : ushort
{
    /// <summary><c>Instruction_Ridley_Roar</c> (queue roar sound) at $A6:E4BE.</summary>
    Roar = 0xe4be,
    /// <summary><c>Instruction_Ridley_ClearRoaringFlag</c> at $A6:E4CA.</summary>
    ClearRoaringFlag = 0xe4ca,
    /// <summary><c>Instruction_Ridley_GotoYIfNotNorfairAndSamusHasLowEnergy</c> (also sets the timer to 8) at $A6:E4D2.</summary>
    GotoYIfNotNorfairAndSamusHasLowEnergy = 0xe4d2,
    /// <summary><c>UNUSED_Instruction_RidleyCeres_GotoYIfNotHoldingBaby_A6E4EE</c> at $A6:E4EE.</summary>
    GotoYIfNotHoldingBaby = 0xe4ee,
    /// <summary><c>UNUSED_Instruction_RidleyCeres_GotoYIfHoldingBaby_A6E4F8</c> at $A6:E4F8.</summary>
    GotoYIfHoldingBaby = 0xe4f8,
    /// <summary><c>Instruction_RidleyCeres_RidleyFeetDistanceIndexInY</c> at $A6:E501.</summary>
    CeresFeetDistanceIndexInY = 0xe501,
    /// <summary><c>Instruction_Ridley_GotoYIfNotFacingLeft</c> at $A6:E517.</summary>
    GotoYIfNotFacingLeft = 0xe517,
    /// <summary><c>Instruction_Ridley_MoveRidleyWithArgsInY</c> at $A6:E51F.</summary>
    MoveWithArgsInY = 0xe51f,
    /// <summary><c>Instruction_Ridley_FlipRidleyLeft</c> (set direction left, update tail parts) at $A6:E71C.</summary>
    FlipLeft = 0xe71c,
    /// <summary><c>Instruction_Ridley_FaceRidleyForward</c> at $A6:E727.</summary>
    FaceForward = 0xe727,
    /// <summary><c>Instruction_Ridley_FlipRidleyRight</c> (set direction right, update tail parts) at $A6:E72F.</summary>
    FlipRight = 0xe72f,
    /// <summary><c>Instruction_Ridley_CalculateFireballXYVelocities</c> at $A6:E84D.</summary>
    CalculateFireballXYVelocities = 0xe84d,
    /// <summary><c>Instruction_Ridley_SpawnRidleysFireballWithAfterburn</c> at $A6:E904.</summary>
    SpawnFireballWithAfterburn = 0xe904,
    /// <summary><c>Instruction_Ridley_SpawnRidleysFireballWithoutAfterburn</c> at $A6:E909.</summary>
    SpawnFireballWithoutAfterburn = 0xe909,
    /// <summary><c>Instruction_RidleyCeres_StartLiftoff</c> (set main AI and vertical speed) at $A6:E969.</summary>
    CeresStartLiftoff = 0xe969,
    /// <summary><c>Instruction_Ridley_StartLiftoff</c> (set main AI and vertical speed) at $A6:E976.</summary>
    StartLiftoff = 0xe976,
}

/// <summary>Sound identifiers queued by <see cref="RidleyInstruction"/> routines.</summary>
internal static class RidleyInstructionSounds
{
    /// <summary>Library-two roar $59 queued by <see cref="RidleyInstruction.Roar"/> (maximum six queued).</summary>
    internal const ushort Roar = 0x0059;
}
