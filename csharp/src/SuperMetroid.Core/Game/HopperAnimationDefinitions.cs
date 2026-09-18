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
        new(0xaa82, 0xaaa8, 0xaa76, 0xaa9c),
        new(0xb0d1, 0xb0f7, 0xb0c5, 0xb0eb),
        new(0xb23f, 0xb25d, 0xb237, 0xb255),
        new(0xafad, 0xafcb, 0xafa5, 0xafc3),
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
