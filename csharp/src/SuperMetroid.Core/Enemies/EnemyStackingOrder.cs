namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The order the enemies' standings stand in (#2618): front to back, and which of them stand in front of Samus and her
/// shots. Enemies of one standing stand in the order the enemy pass queued them.
/// </summary>
internal static partial class EnemyStackingOrder
{
    /// <summary>Every standing, front to back.</summary>
    internal static IReadOnlyList<EnemyStacking> FrontToBack { get; } = Ensure.EveryMemberOnce<EnemyStacking>(
    [
        EnemyStacking.Frontmost,
        EnemyStacking.InFrontOfSamus,
        EnemyStacking.JustBehindSamus,
        EnemyStacking.BehindSamus,
        EnemyStacking.FarBehindSamus,
        EnemyStacking.Rearmost,
    ]);

    /// <summary>The standings in front of Samus and her shots; every other stands behind them.</summary>
    internal static IReadOnlySet<EnemyStacking> InFrontOfSamus { get; } = new HashSet<EnemyStacking>
    {
        EnemyStacking.Frontmost,
        EnemyStacking.InFrontOfSamus,
    };
}
