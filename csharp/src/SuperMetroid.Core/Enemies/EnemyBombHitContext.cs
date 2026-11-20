using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A bomb entering common bomb damage whose enemy's health has been sampled before the damage: the context the
/// stages around common damage receive, which read how much the hit took.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed record EnemyBombHitContext : EnemyBombContext
{
    /// <summary>Takes <paramref name="bomb"/> once its enemy's health before common bomb damage is <paramref name="healthBefore"/>.</summary>
    [SetsRequiredMembers]
    internal EnemyBombHitContext(EnemyBombContext bomb, int healthBefore)
        : base(bomb) => HealthBefore = healthBefore;

    /// <summary>Enemy health before common bomb damage.</summary>
    internal int HealthBefore { get; }
}
