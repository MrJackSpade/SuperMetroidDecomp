using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The parts that show frames on each boss's body, in the order they are drawn (#1540): the first beneath the others. Each
/// boss's body is drawn beneath every other part, as it owns the room's first enemy place; the other parts show their
/// frames on places of the body no other of them reaches. Phantoon's eye shows in his body's socket and his tentacles and
/// mouth beneath it; Draygon's eye and his dying arms show on his body, and his tail shows nothing there; Crocomire's body
/// shows his body frames and melt images, and Mother Brain's her poses. Kraid draws his own body and head.
/// </summary>
internal static class BossBodyPartDefinitions
{
    /// <summary>Each boss's parts, in draw order.</summary>
    internal static IReadOnlyDictionary<BossBodyOwner, ImmutableArray<EnemyDefinitionId>> DrawOrders { get; } =
        new Dictionary<BossBodyOwner, ImmutableArray<EnemyDefinitionId>>
        {
            [BossBodyOwner.Phantoon] = [EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonEye, EnemyDefinitionId.PhantoonTentacles, EnemyDefinitionId.PhantoonMouth],
            [BossBodyOwner.Draygon] = [EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonArms],
            [BossBodyOwner.Crocomire] = [EnemyDefinitionId.Crocomire],
            [BossBodyOwner.MotherBrain] = [EnemyDefinitionId.MotherBrainBody],
        };
}
