namespace SuperMetroid.Core.Game;

/// <summary>Authored animation-list selectors for one Sidehopper/Dessgeega variant.</summary>
internal readonly record struct HopperAnimationDefinition(
    ushort LandedFloor,
    ushort LandedCeiling,
    ushort JumpingFloor,
    ushort JumpingCeiling);

/// <summary>Compiled animation identities shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>
    /// The four variant columns from <c>$A3:AAC2-$AAE1</c>. Native stores these as four
    /// parallel landed-floor, landed-ceiling, jumping-floor, and jumping-ceiling tables.
    /// </summary>
    private static readonly HopperAnimationDefinition[] Variants =
    [
        new(
            HopperInstructionProgramDefinitions.SidehopperLandedFloor,
            HopperInstructionProgramDefinitions.SidehopperLandedCeiling,
            HopperInstructionProgramDefinitions.SidehopperJumpingFloor,
            HopperInstructionProgramDefinitions.SidehopperJumpingCeiling),
        new(
            HopperInstructionProgramDefinitions.LargeSidehopperLandedFloor,
            HopperInstructionProgramDefinitions.LargeSidehopperLandedCeiling,
            HopperInstructionProgramDefinitions.LargeSidehopperJumpingFloor,
            HopperInstructionProgramDefinitions.LargeSidehopperJumpingCeiling),
        new(
            HopperInstructionProgramDefinitions.LargeDessgeegaLandedFloor,
            HopperInstructionProgramDefinitions.LargeDessgeegaLandedCeiling,
            HopperInstructionProgramDefinitions.LargeDessgeegaJumpingFloor,
            HopperInstructionProgramDefinitions.LargeDessgeegaJumpingCeiling),
        new(
            HopperInstructionProgramDefinitions.DessgeegaLandedFloor,
            HopperInstructionProgramDefinitions.DessgeegaLandedCeiling,
            HopperInstructionProgramDefinitions.DessgeegaJumpingFloor,
            HopperInstructionProgramDefinitions.DessgeegaJumpingCeiling),
    ];

    /// <summary>Selects the exact list used by one orientation and movement phase.</summary>
    internal static ushort InstructionList(
        ushort variantIndex,
        bool upsideDown,
        bool jumping)
    {
        if (variantIndex >= Variants.Length)
        {
            throw new InvalidDataException(
                $"Hopper animation variant {variantIndex} exceeds four authored records.");
        }

        HopperAnimationDefinition definition = Variants[variantIndex];
        return (upsideDown, jumping) switch
        {
            (false, false) => definition.LandedFloor,
            (true, false) => definition.LandedCeiling,
            (false, true) => definition.JumpingFloor,
            (true, true) => definition.JumpingCeiling,
        };
    }
}
