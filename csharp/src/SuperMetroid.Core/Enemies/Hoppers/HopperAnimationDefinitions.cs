namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Animation dispatch shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>$A3:AAC2-AAE1: selects the species/size program for its orientation and movement phase.</summary>
    internal static HopperProgram Program(int variantIndex, HopperSurface surface, HopperPhase phase) =>
        (variantIndex, surface, phase) switch
        {
            (0, HopperSurface.Floor, HopperPhase.Jumping) => HopperProgram.SidehopperFloorJumping,
            (0, HopperSurface.Floor, HopperPhase.Landed) => HopperProgram.SidehopperFloorLanded,
            (0, HopperSurface.Ceiling, HopperPhase.Jumping) => HopperProgram.SidehopperCeilingJumping,
            (0, HopperSurface.Ceiling, HopperPhase.Landed) => HopperProgram.SidehopperCeilingLanded,
            (1, HopperSurface.Floor, HopperPhase.Jumping) => HopperProgram.LargeSidehopperFloorJumping,
            (1, HopperSurface.Floor, HopperPhase.Landed) => HopperProgram.LargeSidehopperFloorLanded,
            (1, HopperSurface.Ceiling, HopperPhase.Jumping) => HopperProgram.LargeSidehopperCeilingJumping,
            (1, HopperSurface.Ceiling, HopperPhase.Landed) => HopperProgram.LargeSidehopperCeilingLanded,
            (2, HopperSurface.Floor, HopperPhase.Jumping) => HopperProgram.LargeDessgeegaFloorJumping,
            (2, HopperSurface.Floor, HopperPhase.Landed) => HopperProgram.LargeDessgeegaFloorLanded,
            (2, HopperSurface.Ceiling, HopperPhase.Jumping) => HopperProgram.LargeDessgeegaCeilingJumping,
            (2, HopperSurface.Ceiling, HopperPhase.Landed) => HopperProgram.LargeDessgeegaCeilingLanded,
            (3, HopperSurface.Floor, HopperPhase.Jumping) => HopperProgram.DessgeegaFloorJumping,
            (3, HopperSurface.Floor, HopperPhase.Landed) => HopperProgram.DessgeegaFloorLanded,
            (3, HopperSurface.Ceiling, HopperPhase.Jumping) => HopperProgram.DessgeegaCeilingJumping,
            (3, HopperSurface.Ceiling, HopperPhase.Landed) => HopperProgram.DessgeegaCeilingLanded,
            _ => throw new InvalidDataException($"Hopper animation variant {variantIndex} exceeds four authored records."),
        };
}
