// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>
/// Instance fields a later build removed from serialized types because nothing read them. An older
/// debugger state still names them; restoration reads and discards exactly these values. Any other
/// unknown field remains a layout mismatch and is rejected.
/// </summary>
internal static class DebuggerRetiredFieldDefinitions
{
    /// <summary>(Declaring type full name, field name) of every retired field.</summary>
    private static readonly HashSet<(string Type, string Field)> Retired =
    [
        ("SuperMetroid.Core.Frontend.FileSelectMapScroll", "customButtons"),
        ("SuperMetroid.Core.Game.CrocomireDeathState", "<RumbleIndex>k__BackingField"),
        ("SuperMetroid.Core.Game.KraidEnemyState", "<Unknown4>k__BackingField"),
        ("SuperMetroid.Core.Game.MotherBrainBodyAnimationState", "<Bg2XScroll>k__BackingField"),
        ("SuperMetroid.Core.Game.MotherBrainBodyAnimationState", "<Bg2YScroll>k__BackingField"),
        ("SuperMetroid.Core.Game.MotherBrainBodyAnimationState", "<SpritemapPointer>k__BackingField"),
        ("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackSequence", "<HeadSpritemapPointer>k__BackingField"),
        ("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackSequence", "<OnionRingTargetAngle>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<AttackTableIndex>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<FireballCooldown>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<FireballVolleyCounter>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<GrabbedSamusMovementIndex>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<GrabbedSamusMovementLagTimer>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<PogoTargetX>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<PreviousSamusX>k__BackingField"),
        ("SuperMetroid.Core.Game.RidleyEnemyState", "<SamusMovementDirection>k__BackingField"),
        ("SuperMetroid.Core.Rooms.RoomPlmSystem", "_bombTorizoHandWasDeleted"),
        ("SuperMetroid.Core.Rooms.RoomPlmSystem", "_bombTorizoHandWasLoaded"),
        ("SuperMetroid.Core.Rooms.RoomPlmSystem", "_motherBrainGlassWasDeleted"),
        ("SuperMetroid.Core.Rooms.RoomPlmSystem+EyeDoorPlmState", "<Orientation>k__BackingField"),
    ];

    internal static bool Contains(Type declaringType, string field) => Retired.Contains((declaringType.FullName!, field));
}
