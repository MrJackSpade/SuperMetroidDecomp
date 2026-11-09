using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Verification's native view of the doors (#1414, #1554): the cartridge routine words of the pre-instructions the door
/// lists install. Core's doors hold what they wait for as domain state; a native snapshot or ROM oracle translates through
/// these words.
/// </summary>
internal static class DoorsNativeWords
{
    /// <summary>The bank-$84 shot wake each colour's cap installs as it starts waiting.</summary>
    internal static IReadOnlyDictionary<ColoredDoorColor, ushort> ColoredDoorShotWakes { get; } = new Dictionary<ColoredDoorColor, ushort>
    {
        [ColoredDoorColor.Yellow] = 0xbd26,
        [ColoredDoorColor.Green] = 0xbd88,
        [ColoredDoorColor.Red] = 0xbd50,
    };

    /// <summary>The bank-$84 routine word naming each <see cref="EyeDoorFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<EyeDoorFrameBehavior> EyeDoorFrameBehaviors = new(new Dictionary<EyeDoorFrameBehavior, ushort>
    {
        [EyeDoorFrameBehavior.WakeWhenDoorBitSet] = 0xd753,
        [EyeDoorFrameBehavior.MissileHit] = 0xbd50,
    });

    /// <summary>The bank-$84 any-shot wake an unlocked grey cap installs.</summary>
    internal const ushort GreyDoorShotWake = 0xbd0f;

    /// <summary>The bank-$84 condition wake a locked grey cap installs for each condition.</summary>
    internal static IReadOnlyDictionary<GreyDoorCondition, ushort> GreyDoorConditionWakes { get; } = new Dictionary<GreyDoorCondition, ushort>
    {
        [GreyDoorCondition.AreaBossDefeated] = 0xbdd4,
        [GreyDoorCondition.AreaMiniBossDefeated] = 0xbde3,
        [GreyDoorCondition.AreaTorizoDefeated] = 0xbdf2,
        [GreyDoorCondition.EnemyDeathQuota] = 0xbe01,
        [GreyDoorCondition.Never] = 0xbe1c,
        [GreyDoorCondition.TourianStatueFinished] = 0xbe1f,
        [GreyDoorCondition.CrittersEscaped] = 0xbe30,
    };

    /// <summary>
    /// The native pre-instruction a grey cap that waits for <paramref name="wait"/> runs: its condition's wake, the any-shot
    /// wake, or the spawn's default when it waits for nothing.
    /// </summary>
    internal static ushort GreyDoorWaitWord(GreyDoorWait wait, GreyDoorCondition condition) => wait switch
    {
        GreyDoorWait.Nothing => RoomObjectFrameBehaviorDefinitions.SpawnDefault,
        GreyDoorWait.Condition => GreyDoorConditionWakes[condition],
        GreyDoorWait.Shot => GreyDoorShotWake,
        _ => throw new InvalidOperationException($"Undefined {nameof(GreyDoorWait)} {wait}."),
    };
}
