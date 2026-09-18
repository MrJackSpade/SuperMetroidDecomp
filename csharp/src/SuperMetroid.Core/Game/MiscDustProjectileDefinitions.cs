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
    /// <c>$86:E42C-$E467</c>, the complete thirty-word instruction-list selector table
    /// consumed by initializer <c>$86:E468</c>.
    /// </summary>
    private static readonly ushort[] InstructionLists =
    [
        0xe0ee, 0xe100, 0xe11a, 0xe138, 0xe152, 0xe168, 0xe17e, 0xe198,
        0xe1a6, 0xe1b0, 0xe1c6, 0xe1d8, 0xe1ea, 0xe222, 0xe234, 0xe246,
        0xe258, 0xe266, 0xe2a8, 0xe2ba, 0xe2d4, 0xe2f2, 0xe314, 0xe392,
        0xe3a0, 0xe3c6, 0xe3e8, 0xe40a, 0xe1fc, 0xe208,
    ];

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
    internal static ushort InstructionList(ushort animationIndex)
    {
        if (animationIndex >= InstructionLists.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex),
                animationIndex,
                "Bank-$86 misc-dust animation index must be in the native $00..$1D range.");
        }

        return InstructionLists[animationIndex];
    }

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
