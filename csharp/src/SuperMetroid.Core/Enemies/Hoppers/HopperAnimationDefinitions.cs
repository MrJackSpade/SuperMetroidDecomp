namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Animation dispatch shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>$A3:AAC2-AAE1: the species/size program for each orientation and movement phase.</summary>
    internal static readonly Dictionary<HopperProgramKey, HopperProgram> Programs = new()
    {
        [new(0, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.SidehopperFloorJumping,
        [new(0, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.SidehopperFloorLanded,
        [new(0, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.SidehopperCeilingJumping,
        [new(0, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.SidehopperCeilingLanded,
        [new(1, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.LargeSidehopperFloorJumping,
        [new(1, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.LargeSidehopperFloorLanded,
        [new(1, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.LargeSidehopperCeilingJumping,
        [new(1, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.LargeSidehopperCeilingLanded,
        [new(2, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.LargeDessgeegaFloorJumping,
        [new(2, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.LargeDessgeegaFloorLanded,
        [new(2, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.LargeDessgeegaCeilingJumping,
        [new(2, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.LargeDessgeegaCeilingLanded,
        [new(3, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.DessgeegaFloorJumping,
        [new(3, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.DessgeegaFloorLanded,
        [new(3, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.DessgeegaCeilingJumping,
        [new(3, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.DessgeegaCeilingLanded,
    };
}
