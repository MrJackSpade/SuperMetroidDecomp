using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled cartridge definitions shared by room-graphics dust and explosions.</summary>
internal static class MiscDustProjectileDefinitions
{
    /// <summary>Returns one of the thirty authored misc-dust animation lists.</summary>
    internal static ushort InstructionList(MiscDustAnimation animation) =>
        EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointer(animation);

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
