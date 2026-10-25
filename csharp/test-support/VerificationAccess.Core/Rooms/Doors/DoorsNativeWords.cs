using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Verification's native view of the door identities (#1414): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DoorsNativeWords
{
    /// <summary>The bank-$84 routine word naming each <see cref="ColoredDoorPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<ColoredDoorPreInstruction> ColoredDoorPreInstructions = new(new Dictionary<ColoredDoorPreInstruction, ushort>
    {
        [ColoredDoorPreInstruction.None] = 0x0000,
        [ColoredDoorPreInstruction.WakeOnPowerBomb] = 0xbd26,
        [ColoredDoorPreInstruction.WakeOnMissile] = 0xbd50,
        [ColoredDoorPreInstruction.WakeOnSuperMissile] = 0xbd88,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="EyeDoorPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<EyeDoorPreInstruction> EyeDoorPreInstructions = new(new Dictionary<EyeDoorPreInstruction, ushort>
    {
        [EyeDoorPreInstruction.WakeWhenDoorBitSet] = 0xd753,
        [EyeDoorPreInstruction.MissileHit] = 0xbd50,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="GreyDoorPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<GreyDoorPreInstruction> GreyDoorPreInstructions = new(new Dictionary<GreyDoorPreInstruction, ushort>
    {
        [GreyDoorPreInstruction.None] = 0x0000,
        [GreyDoorPreInstruction.WakeWhenShot] = 0xbd0f,
        [GreyDoorPreInstruction.WakeIfAreaBossDead] = 0xbdd4,
        [GreyDoorPreInstruction.WakeIfAreaMiniBossDead] = 0xbde3,
        [GreyDoorPreInstruction.WakeIfAreaTorizoDead] = 0xbdf2,
        [GreyDoorPreInstruction.WakeIfEnemiesDead] = 0xbe01,
        [GreyDoorPreInstruction.PlayDudSound] = 0xbe1c,
        [GreyDoorPreInstruction.WakeIfTourianStatueFinished] = 0xbe1f,
        [GreyDoorPreInstruction.WakeIfCrittersEscaped] = 0xbe30,
    });
}
