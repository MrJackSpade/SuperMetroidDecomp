using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which Torizo occupant an enemy place holds, one per definition the species installs.</summary>
internal enum TorizoPart
{
    /// <summary>The <see cref="EnemyDefinitionId.BombTorizo"/> occupant.</summary>
    BombTorizo,
    /// <summary>The <see cref="EnemyDefinitionId.GoldenTorizo"/> occupant.</summary>
    GoldenTorizo,
}
