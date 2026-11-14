namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Which block of an enemy's span a terrain move is testing: the enemy's side spans one or more blocks, and a square slope
/// in the first or last of them meets only the half of the enemy that reaches into it.
/// </summary>
internal enum EnemyTerrainSpanBlock
{
    /// <summary>A block between the span's first and last.</summary>
    Inner,

    /// <summary>The span's first block: the top one for a horizontal move, the left one for a vertical move.</summary>
    First,

    /// <summary>
    /// The span's last block: the bottom one for a horizontal move, the right one for a vertical move. A span of one block
    /// tests that block as its last.
    /// </summary>
    Last,
}
