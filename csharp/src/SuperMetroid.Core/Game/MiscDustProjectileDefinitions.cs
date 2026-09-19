namespace SuperMetroid.Core.Game;

/// <summary>One randomized room-coordinate placement record for bank-$86 misc dust.</summary>
internal readonly record struct MiscDustPlacementDefinition(
    ushort XMask,
    ushort YMask,
    short XBase,
    short YBase);

/// <summary>Compiled cartridge definitions shared by room-graphics dust and explosions.</summary>
internal static class MiscDustProjectileDefinitions
{
    /// <summary>
    /// <c>$86:E47E-$E4A5</c>, the five randomized X/Y mask-and-base records consumed by
    /// eye-door smoke initializer <c>$86:E4A6</c>.
    /// </summary>
    private static readonly MiscDustPlacementDefinition[] SmokePlacements =
    [
        new(0x0000, 0x0000, 0x0000, 0x0000),
        new(0x0007, 0x0007, -4, -4),
        new(0x000f, 0x000f, -8, -8),
        new(0x000f, 0x001f, -8, -16),
        new(0x001f, 0x003f, -16, -32),
    ];

    /// <summary>Returns one of the thirty authored misc-dust animation lists.</summary>
    internal static ushort InstructionList(ushort animationIndex) =>
        EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointer(animationIndex);

    /// <summary>Returns one of the five authored randomized smoke-placement records.</summary>
    internal static MiscDustPlacementDefinition SmokePlacement(ushort placementIndex)
    {
        if (placementIndex >= SmokePlacements.Length)
        {
            throw new InvalidDataException(
                $"Bank-$86 misc-dust placement index {placementIndex} exceeds five authored records.");
        }

        return SmokePlacements[placementIndex];
    }
}
