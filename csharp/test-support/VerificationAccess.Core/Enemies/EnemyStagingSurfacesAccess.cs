using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to <see cref="EnemyStagingSurfaces"/> members production does not use.</summary>
internal static class EnemyStagingSurfacesAccess
{
    /// <summary>Exposes <see cref="EnemyStagingSurfaces"/> state that only verification reads.</summary>
    extension(EnemyStagingSurfaces self)
    {
        /// <summary>The 4bpp corpse tile surface the dead-monster and Dead Torizo rotting routines move pixel rows in.</summary>
        public StagingByteView CorpseTiles => self.Bg2.ByteView(0, self.Bg2.Length);
        /// <summary>Mother Brain's six-row corpse tile staging buffer, uploaded by <c>$A9:E1F4</c>.</summary>
        public StagingByteView MotherBrainCorpseTiles =>
            self.RottingHeap.ByteView(0, EnemyStagingDefinitionsConstants.MotherBrainCorpseTileByteCount);
        /// <summary>The Dead Torizo sand strips that <c>$A9:D69C</c> rewrites line by line.</summary>
        public StagingByteView DeadTorizoSand =>
            self.RottingHeap.ByteView(EnemyStagingDefinitions.DeadTorizoSandOffset, EnemyStagingDefinitions.DeadTorizoSandByteCount);
    }
}
