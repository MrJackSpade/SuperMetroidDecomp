using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>One Samus/enemy overlap that reached a touch reaction.</summary>
/// <param name="Occupant">The touched enemy, which a killing touch kills.</param>
/// <param name="Samus">Samus, who touched it.</param>
/// <param name="PseudoScrew">Samus's held charge, which her Pseudo Screw contact spends (#2159).</param>
/// <param name="ControllerInput">Held input, used by reactions that grab or eject Samus.</param>
/// <param name="Level">Room terrain for collision-aware reactions.</param>
/// <param name="UsesFrameHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HitboxTouch">The named reaction of the overlapping hitbox (the definition's on the radius path), if named.</param>
/// <param name="Camera">The camera's position as the touch pass runs.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyTouchContext(
    RoomEnemyOccupant Occupant,
    SamusState Samus,
    PseudoScrewCharge PseudoScrew,
    GameplayAction ControllerInput,
    RoomLevelData Level,
    bool UsesFrameHitboxes,
    EnemyReaction? HitboxTouch,
    Position16<RoomPixels> Camera)
{
    /// <summary>The place the touched occupant was spawned in, which the reaction reads and writes.</summary>
    internal RoomEnemyPlace Place => Occupant.Place;
}
