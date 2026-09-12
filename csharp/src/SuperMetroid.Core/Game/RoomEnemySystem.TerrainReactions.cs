using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    // This is a synchronous call context, not emulated persistent state. Every enemy
    // frame supplies its room's existing PLM owner, including after debugger restoration.
    [NonSerialized] private RoomPlmSystem? _collisionPlms;

    private bool ReactToEnemySpikeBlock(RoomLevelData level, int blockIndex, RoomCollisionBlock block)
    {
        ushort header = ReadWord(_bus!, EnemyBreakableTerrainDefinitions.ReactionTable +
            (block.Behavior & EnemyBreakableTerrainDefinitions.ReactionIndexMask) * 2);
        if (header == 0) return true;
        if (header != EnemyBreakableTerrainDefinitions.Header)
            throw new NotSupportedException($"Enemy spike BTS ${block.Behavior:X2} selects untranslated PLM ${header:X4}.");
        var plms = _collisionPlms ?? throw new InvalidOperationException("Enemy-breakable terrain requires the active room PLM owner.");
        plms.TrySpawnEnemyBreakableBlock(level, blockIndex);
        return false; // $A0:C2D6 clears carry even if the PLM pool was full.
    }

    private readonly struct EnemyTerrainScope : IDisposable
    {
        private readonly RoomEnemySystem owner;
        private readonly RoomPlmSystem? previous;
        public EnemyTerrainScope(RoomEnemySystem owner, RoomPlmSystem? current)
        {
            this.owner = owner;
            previous = owner._collisionPlms;
            owner._collisionPlms = current;
        }
        public void Dispose() => owner._collisionPlms = previous;
    }
}
