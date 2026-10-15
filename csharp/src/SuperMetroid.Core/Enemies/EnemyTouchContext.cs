using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies;

/// <summary>One Samus/enemy overlap that reached a translated touch reaction.</summary>
/// <param name="Slot">The touched enemy.</param>
/// <param name="Samus">Samus, who touched it.</param>
/// <param name="ControllerInput">Held input, used by reactions that grab or eject Samus.</param>
/// <param name="Level">Room terrain for collision-aware reactions.</param>
/// <param name="UsesExtendedHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HitboxTouchAi">The named callback of the overlapping hitbox (the header's on the radius path), if named.</param>
/// <param name="HitboxTouchAiPointer">The raw bank-local callback word of the overlapping hitbox.</param>
/// <param name="ContactDamageIndex">Samus's contact-damage index sampled at the pass entry.</param>
internal readonly record struct EnemyTouchContext(
    RoomEnemySlot Slot,
    SamusState Samus,
    SnesButton ControllerInput,
    RoomLevelData? Level,
    bool UsesExtendedHitboxes,
    EnemyInteractionCallback? HitboxTouchAi,
    ushort HitboxTouchAiPointer,
    SamusContactDamageIndex ContactDamageIndex);
