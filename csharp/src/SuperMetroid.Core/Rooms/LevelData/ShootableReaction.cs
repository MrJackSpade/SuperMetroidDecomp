namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a shootable block or shootable air does with its behaviour (#1722, #1742): the reaction the shot, bomb and grapple
/// responses select. Its values are arbitrary; a behaviour written for another collision kind is read as one of these
/// through <see cref="RoomBlockBehavior"/>, or as none when the shootable kinds have no reaction for it.
/// </summary>
public enum ShootableReaction
{
    /// <summary>Any weapon breaks one block, and it grows back.</summary>
    RespawningBreak1x1,

    /// <summary>Any weapon breaks two blocks side by side, and they grow back.</summary>
    RespawningBreak2x1,

    /// <summary>Any weapon breaks two stacked blocks, and they grow back.</summary>
    RespawningBreak1x2,

    /// <summary>Any weapon breaks a two-by-two square of blocks, and it grows back.</summary>
    RespawningBreak2x2,

    /// <summary>Any weapon breaks one block for good.</summary>
    PermanentBreak1x1,

    /// <summary>Any weapon breaks two blocks side by side for good.</summary>
    PermanentBreak2x1,

    /// <summary>Any weapon breaks two stacked blocks for good.</summary>
    PermanentBreak1x2,

    /// <summary>Any weapon breaks a two-by-two square of blocks for good.</summary>
    PermanentBreak2x2,

    /// <summary>A Power Bomb breaks the block, and it grows back; a normal bomb shows it for an update.</summary>
    RespawningPowerBombBlock,

    /// <summary>A Power Bomb breaks the block for good; a normal bomb shows it for an update.</summary>
    PermanentPowerBombBlock,

    /// <summary>A Super Missile breaks the block, and it grows back; a normal bomb shows it for an update.</summary>
    RespawningSuperMissileBlock,

    /// <summary>A Super Missile breaks the block for good; a normal bomb shows it for an update.</summary>
    PermanentSuperMissileBlock,

    /// <summary>
    /// Every weapon and the grapple beam set off a one-update object that leaves the block alone; the X-ray scope reveals
    /// the block.
    /// </summary>
    Nothing,

    /// <summary>The collision probe a closed downward gate gives its column: it publishes the shot's collision result.</summary>
    CollisionProbe,

    /// <summary>
    /// Shots pass it by without a reaction and the X-ray scope does not reveal it; the grapple beam sets off the one-update
    /// object that leaves the block alone, and a bomb has no reaction for it.
    /// </summary>
    IgnoresShots,

    /// <summary>The left-facing blue door cap, which a shot opens.</summary>
    BlueDoorCapLeft,

    /// <summary>The right-facing blue door cap, which a shot opens.</summary>
    BlueDoorCapRight,

    /// <summary>The upward-facing blue door cap, which a shot opens.</summary>
    BlueDoorCapUp,

    /// <summary>The downward-facing blue door cap, which a shot opens.</summary>
    BlueDoorCapDown,

    /// <summary>
    /// The trigger a resident room object (a coloured door, an eye door, a glass tube, a cannon) gives its blocks: a hit is
    /// passed to the object.
    /// </summary>
    ResidentObjectTrigger,

    /// <summary>The trigger a collectible gives its block: a hit is passed to the collectible.</summary>
    CollectibleTrigger,

    /// <summary>The blue downward gate's left trigger.</summary>
    GateTriggerBlueLeft,

    /// <summary>The blue downward gate's right trigger.</summary>
    GateTriggerBlueRight,

    /// <summary>The red downward gate's left trigger.</summary>
    GateTriggerRedLeft,

    /// <summary>The red downward gate's right trigger.</summary>
    GateTriggerRedRight,

    /// <summary>The green downward gate's left trigger.</summary>
    GateTriggerGreenLeft,

    /// <summary>The green downward gate's right trigger.</summary>
    GateTriggerGreenRight,

    /// <summary>The yellow downward gate's left trigger.</summary>
    GateTriggerYellowLeft,

    /// <summary>The yellow downward gate's right trigger.</summary>
    GateTriggerYellowRight,

    /// <summary>The wall the escape animals break out through.</summary>
    AnimalEscape,

    /// <summary>
    /// A shootable block sets off the one-update object that leaves it alone for any weapon or the grapple beam; shootable
    /// air lets a weapon pass with no reaction at all (<see cref="ShotBlockReactions"/>).
    /// </summary>
    NothingWhenSolid,
}
