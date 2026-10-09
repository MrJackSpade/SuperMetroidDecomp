using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    // This is a synchronous call context, not emulated persistent state. Every enemy
    // frame supplies its room's existing PLM owner, including after debugger restoration.
    /// <summary>Room PLM owner active during collision callbacks, used to spawn enemy-breakable terrain blocks.</summary>
    [NonSerialized] private RoomPlmSystem? _collisionPlms;

    /// <summary>Spawns a room PLM for enemy-breakable terrain and reports whether ordinary block collision should continue.</summary>
    /// <param name="level">Level data whose PLM system owns the resulting block effect.</param>
    /// <param name="blockIndex">Index of the collided block in the room map.</param>
    /// <param name="block">Collision block whose behavior determines whether it is enemy-breakable.</param>
    /// <returns><see langword="true"/> when the block uses normal collision; <see langword="false"/> when the breakable-block path consumes it.</returns>
    /// <exception cref="InvalidOperationException">The block is enemy-breakable but no active room PLM owner was supplied for this collision callback.</exception>
    private bool ReactToEnemySpikeBlock(RoomLevelData level, int blockIndex, RoomCollisionBlock block)
    {
        if (!EnemyBreakableTerrainDefinitions.IsEnemyBreakable(block.Behavior)) return true;
        var plms = _collisionPlms ?? throw new InvalidOperationException("Enemy-breakable terrain requires the active room PLM owner.");
        plms.TrySpawnEnemyBreakableBlock(level, blockIndex);
        return false; // $A0:C2D6 clears carry even if the PLM pool was full.
    }

    /// <summary>Temporarily installs a room PLM owner for enemy-terrain collision callbacks and restores the prior owner on disposal.</summary>
    private readonly struct EnemyTerrainScope : IDisposable
    {
        /// <summary>Enemy system whose collision callback context is temporarily changed.</summary>
        private readonly RoomEnemySystem owner;
        /// <summary>PLM owner that was active before this scope began.</summary>
        private readonly RoomPlmSystem? previous;

        /// <summary>Sets the active PLM owner while retaining the previous value for restoration.</summary>
        /// <param name="owner">Enemy system receiving the temporary collision context.</param>
        /// <param name="current">Room PLM system to use during the scope, or null when no room owner is available.</param>
        public EnemyTerrainScope(RoomEnemySystem owner, RoomPlmSystem? current)
        {
            this.owner = owner;
            previous = owner._collisionPlms;
            owner._collisionPlms = current;
        }
        /// <summary>Restores the enemy system's collision context to the owner that was active before this scope.</summary>
        public void Dispose() => owner._collisionPlms = previous;
    }
}
