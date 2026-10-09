namespace SuperMetroid.Core.Game;

/// <summary>One randomized room-coordinate placement record for bank-$86 misc dust.</summary>
/// <param name="XMask">Mask applied to the low random-number bits to select a horizontal offset.</param>
/// <param name="YMask">Mask applied to the upper random-number bits to select a vertical offset.</param>
/// <param name="XBase">Signed horizontal offset added before the masked random component.</param>
/// <param name="YBase">Signed vertical offset added before the masked random component.</param>
internal readonly record struct MiscDustPlacementDefinition(
    ushort XMask,
    ushort YMask,
    short XBase,
    short YBase);

/// <summary>Compiled cartridge definitions shared by room-graphics dust and explosions.</summary>
internal static class MiscDustProjectileDefinitions
{
    /// <summary>Returns one of the thirty authored misc-dust animation lists.</summary>
    internal static ushort InstructionList(ushort animationIndex) =>
        EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointer(animationIndex);

    /// <summary>$86:E47E: point, centered 8/16-pixel squares, then centered 16x32/32x64 smoke boxes.</summary>
    internal static MiscDustPlacementDefinition SmokePlacement(ushort placementIndex)
    {
        if (placementIndex >= 5)
            throw new InvalidDataException($"Bank-$86 misc-dust placement index {placementIndex} exceeds five native records.");
        if (placementIndex == 0) return default;
        bool tall = placementIndex >= 3;
        int xRadius = 1 << (placementIndex + (tall ? 0 : 1));
        int yRadius = tall ? xRadius * 2 : xRadius;
        return new((ushort)(xRadius * 2 - 1), (ushort)(yRadius * 2 - 1), (short)-xRadius, (short)-yRadius);
    }
}
