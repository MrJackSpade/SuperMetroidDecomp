namespace SuperMetroid.Core.Game;

/// <summary>Compiled parallel records used by the shared Bomb/Golden Torizo initializer.</summary>
public static class TorizoInitializationDefinitions
{
    /// <summary>
    /// $AA:C95F..C976, Torizo_Init parallel X/Y, instruction, property and radius tables.
    /// First entries select Bomb Torizo, including InstList_Torizo_BombTorizo_Initial_0
    /// ($AA:B879). The property word is ORed into the population's existing properties.
    /// </summary>
    public static TorizoInitializationDefinition Bomb => new(0x00db, 0x00b3, 0xb879, 0x2800, 0x12, 0x30);

    /// <summary>
    /// Second entries of $AA:C95F..C976 select Golden Torizo, including
    /// InstList_GoldenTorizo_Initial_0 ($AA:C9CB). Its shorter Y radius is authored data.
    /// </summary>
    public static TorizoInitializationDefinition Golden => new(0x01a8, 0x0090, 0xc9cb, 0x2800, 0x12, 0x29);
}

/// <summary>Initial actor placement, program selection, property mask and collision radii.</summary>
public readonly record struct TorizoInitializationDefinition(
    ushort X, ushort Y, ushort Instruction, ushort PropertyMask, ushort XRadius, ushort YRadius);
