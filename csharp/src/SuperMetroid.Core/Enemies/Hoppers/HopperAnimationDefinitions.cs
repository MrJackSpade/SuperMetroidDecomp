namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Animation dispatch shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>$A3:AAC2-AAE1: selects the species/size program for its orientation and movement phase.</summary>
    internal static HopperProgram Program(ushort variantIndex, bool upsideDown, bool jumping) =>
        (variantIndex, upsideDown, jumping) switch
        {
            (0, false, true) => HopperProgram.SidehopperFloorJumping,
            (0, false, false) => HopperProgram.SidehopperFloorLanded,
            (0, true, true) => HopperProgram.SidehopperCeilingJumping,
            (0, true, false) => HopperProgram.SidehopperCeilingLanded,
            (1, false, true) => HopperProgram.LargeSidehopperFloorJumping,
            (1, false, false) => HopperProgram.LargeSidehopperFloorLanded,
            (1, true, true) => HopperProgram.LargeSidehopperCeilingJumping,
            (1, true, false) => HopperProgram.LargeSidehopperCeilingLanded,
            (2, false, true) => HopperProgram.LargeDessgeegaFloorJumping,
            (2, false, false) => HopperProgram.LargeDessgeegaFloorLanded,
            (2, true, true) => HopperProgram.LargeDessgeegaCeilingJumping,
            (2, true, false) => HopperProgram.LargeDessgeegaCeilingLanded,
            (3, false, true) => HopperProgram.DessgeegaFloorJumping,
            (3, false, false) => HopperProgram.DessgeegaFloorLanded,
            (3, true, true) => HopperProgram.DessgeegaCeilingJumping,
            (3, true, false) => HopperProgram.DessgeegaCeilingLanded,
            _ => throw new InvalidDataException($"Hopper animation variant {variantIndex} exceeds four authored records."),
        };
}
