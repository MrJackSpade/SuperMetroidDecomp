using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The sounds and terrain an eye door's components install.</summary>
internal static class EyeDoorDefinitions
{
    /// <summary>The sound the eye's shot makes.</summary>
    public static readonly SoundEffectId ProjectileSound = Library2Sound.AcidSpit;

    /// <summary>The sound of a shot the eye shrugs off.</summary>
    public static readonly SoundEffectId RejectedShotSound = Library2Sound.DudShotOnDoor;

    /// <summary>The collision of the live eye: a shootable block.</summary>
    public const RoomCollisionType EyeCollision = RoomCollisionType.ShootableBlock;

    /// <summary>The live eye's behaviour: shots that hit it notify the eye door.</summary>
    public static readonly RoomBlockBehavior EyeBehavior = RoomBlockBehaviorValues.ResidentPlmProjectileTrigger;

    /// <summary>The collision the passive door components install: a spike-type solid block.</summary>
    public const RoomCollisionType ClosedComponentCollision = RoomCollisionType.SpikeBlock;

    /// <summary>The passive components' behaviour: none.</summary>
    public static readonly RoomBlockBehavior ClosedComponentBehavior = RoomBlockBehaviorValues.None;

    /// <summary>Vertical extension rows the live eye installs below itself, each resolving to the eye.</summary>
    public const int EyeExtensionRows = 1;

    /// <summary>Number of vertical extension cells constructed beneath the blue-door cap.</summary>
    public const int BlueDoorExtensionCount = 3;
}
