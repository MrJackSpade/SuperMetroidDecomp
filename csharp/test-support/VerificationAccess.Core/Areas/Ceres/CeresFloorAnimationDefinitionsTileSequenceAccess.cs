
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresFloorAnimation.TileSequence"/> members production does not use.</summary>
internal static class CeresFloorAnimationDefinitionsTileSequenceAccess
{
    /// <summary>Exposes the number of tile numbers in a Ceres floor run.</summary>
    extension(CeresFloorAnimation.TileSequence self)
    {
        /// <summary>Number of tile numbers in this run.</summary>
        internal int Length => self.Count;
    }
}
