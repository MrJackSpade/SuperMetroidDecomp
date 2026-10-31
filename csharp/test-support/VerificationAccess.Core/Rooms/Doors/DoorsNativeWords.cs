using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Verification's native view of the door identities (#1414): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DoorsNativeWords
{
    /// <summary>The bank-$84 routine word naming each <see cref="ColoredDoorFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<ColoredDoorFrameBehavior> ColoredDoorFrameBehaviors = new(new Dictionary<ColoredDoorFrameBehavior, ushort>
    {
        [ColoredDoorFrameBehavior.None] = 0x0000,
        [ColoredDoorFrameBehavior.WakeOnPowerBomb] = 0xbd26,
        [ColoredDoorFrameBehavior.WakeOnMissile] = 0xbd50,
        [ColoredDoorFrameBehavior.WakeOnSuperMissile] = 0xbd88,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="EyeDoorFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<EyeDoorFrameBehavior> EyeDoorFrameBehaviors = new(new Dictionary<EyeDoorFrameBehavior, ushort>
    {
        [EyeDoorFrameBehavior.WakeWhenDoorBitSet] = 0xd753,
        [EyeDoorFrameBehavior.MissileHit] = 0xbd50,
    });

    /// <summary>The bank-$84 routine word naming each <see cref="GreyDoorFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<GreyDoorFrameBehavior> GreyDoorFrameBehaviors = new(new Dictionary<GreyDoorFrameBehavior, ushort>
    {
        [GreyDoorFrameBehavior.None] = 0x0000,
        [GreyDoorFrameBehavior.WakeWhenShot] = 0xbd0f,
        [GreyDoorFrameBehavior.WakeIfAreaBossDead] = 0xbdd4,
        [GreyDoorFrameBehavior.WakeIfAreaMiniBossDead] = 0xbde3,
        [GreyDoorFrameBehavior.WakeIfAreaTorizoDead] = 0xbdf2,
        [GreyDoorFrameBehavior.WakeIfEnemiesDead] = 0xbe01,
        [GreyDoorFrameBehavior.PlayDudSound] = 0xbe1c,
        [GreyDoorFrameBehavior.WakeIfTourianStatueFinished] = 0xbe1f,
        [GreyDoorFrameBehavior.WakeIfCrittersEscaped] = 0xbe30,
    });
}
