using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A shot past its impact whose enemy's health has been sampled before vulnerability damage: the context the
/// stages from vulnerability on receive, which read how much the hit took.
/// </summary>
internal sealed partial record EnemyShotHitContext : EnemyShotContext
{
    /// <summary>Takes <paramref name="shot"/> once its enemy's health after the impact and before vulnerability damage is <paramref name="healthBefore"/>.</summary>
    [SetsRequiredMembers]
    internal EnemyShotHitContext(EnemyShotContext shot, int healthBefore)
        : base(shot) => HealthBefore = healthBefore;

    /// <summary>Enemy health after the impact and before vulnerability damage.</summary>
    internal int HealthBefore { get; }
}
