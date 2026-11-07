using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="RoomObjectShotBlockDrawDefinitions.DrawList"/> members production does not use.</summary>
internal static class RoomObjectShotBlockDrawDefinitionsDrawListAccess
{
    /// <summary>Exposes a draw list's block appearances.</summary>
    extension(RoomObjectShotBlockDrawDefinitions.DrawList self)
    {
        /// <summary>A new array of every run's block appearances, in run then block order.</summary>
        internal RoomBlockAppearance[] Appearances()
        {
            int count = 0;
            foreach (RoomObjectShotBlockDrawDefinitions.Run run in self.Runs.Span)
                count += run.Count;
            var appearances = new RoomBlockAppearance[count];
            int next = 0;
            foreach (RoomObjectShotBlockDrawDefinitions.Run run in self.Runs.Span)
            {
                foreach (RoomTerrainBlock block in run.Blocks.Span)
                    appearances[next++] = block.Appearance;
            }
            return appearances;
        }
    }
}
