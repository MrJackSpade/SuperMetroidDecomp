using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which BombTorizo occupant a slot holds, one per definition the species installs.</summary>
internal enum BombTorizoPart
{
    /// <summary>The <see cref="EnemyDefinitionId.BombTorizo"/> occupant.</summary>
    BombTorizo,
    /// <summary>The <see cref="EnemyDefinitionId.GoldenTorizo"/> occupant.</summary>
    GoldenTorizo,
}
