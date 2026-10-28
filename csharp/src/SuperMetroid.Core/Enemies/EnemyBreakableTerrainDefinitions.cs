using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Native enemy/spike collision dispatch, <c>EnemyBlockCollisionReaction_Spike.PLMs</c> at <c>$A0:C2DA-$A0:C2F9</c>:
/// the cartridge masks bit seven, treats entries zero through fourteen as solid, and spawns
/// <see cref="PlmHeaderId.EnemyBreakableBlock"/> for entry fifteen.
/// </summary>
public static class EnemyBreakableTerrainDefinitions
{
    /// <summary>The block behavior bits that index the reaction table; bit seven is masked off.</summary>
    internal const int ReactionIndexMask = 0x7f;

    /// <summary>The entries in the reaction table.</summary>
    internal const int ReactionCount = 16;

    /// <summary>The one entry that spawns the enemy-breakable block.</summary>
    internal const int BreakableReaction = 15;
}
