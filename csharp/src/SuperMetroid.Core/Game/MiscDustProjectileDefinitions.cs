namespace SuperMetroid.Core.Game;

/// <summary>One randomized room-coordinate placement record for bank-$86 misc dust.</summary>
/// <param name="RandomMask">Per-axis masks applied to the random word's low (X) and high (Y) bytes; each is the scatter span minus one.</param>
/// <param name="Base">Signed offset added before the masked random scatter.</param>
internal readonly record struct MiscDustPlacementDefinition(
    Extent16<RoomPixels> RandomMask,
    Offset16<RoomPixels> Base);

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
        return new(
            new Extent16<RoomPixels>((ushort)(xRadius * 2 - 1), (ushort)(yRadius * 2 - 1)),
            new Offset16<RoomPixels>((short)-xRadius, (short)-yRadius));
    }
}
