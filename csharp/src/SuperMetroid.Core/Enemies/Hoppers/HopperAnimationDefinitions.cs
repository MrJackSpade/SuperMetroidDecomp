namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Animation dispatch shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>Each kind of hopper's program for each orientation and movement phase.</summary>
    internal static readonly Dictionary<HopperProgramKey, HopperProgram> Programs = new()
    {
        [new(HopperKind.Sidehopper, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.SidehopperFloorJumping,
        [new(HopperKind.Sidehopper, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.SidehopperFloorLanded,
        [new(HopperKind.Sidehopper, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.SidehopperCeilingJumping,
        [new(HopperKind.Sidehopper, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.SidehopperCeilingLanded,
        [new(HopperKind.LargeSidehopper, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.LargeSidehopperFloorJumping,
        [new(HopperKind.LargeSidehopper, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.LargeSidehopperFloorLanded,
        [new(HopperKind.LargeSidehopper, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.LargeSidehopperCeilingJumping,
        [new(HopperKind.LargeSidehopper, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.LargeSidehopperCeilingLanded,
        [new(HopperKind.LargeDessgeega, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.LargeDessgeegaFloorJumping,
        [new(HopperKind.LargeDessgeega, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.LargeDessgeegaFloorLanded,
        [new(HopperKind.LargeDessgeega, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.LargeDessgeegaCeilingJumping,
        [new(HopperKind.LargeDessgeega, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.LargeDessgeegaCeilingLanded,
        [new(HopperKind.Dessgeega, HopperSurface.Floor, HopperPhase.Jumping)] = HopperProgram.DessgeegaFloorJumping,
        [new(HopperKind.Dessgeega, HopperSurface.Floor, HopperPhase.Landed)] = HopperProgram.DessgeegaFloorLanded,
        [new(HopperKind.Dessgeega, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperProgram.DessgeegaCeilingJumping,
        [new(HopperKind.Dessgeega, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperProgram.DessgeegaCeilingLanded,
    };
}
