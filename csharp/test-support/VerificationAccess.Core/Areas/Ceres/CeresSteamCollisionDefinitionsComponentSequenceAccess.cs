
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresSteamCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class CeresSteamCollisionDefinitionsComponentSequenceAccess
{
    /// <summary>Exposes the component count of one compiled Ceres steam collision frame.</summary>
    extension(CeresSteamCollisionDefinitions.ComponentSequence self)
    {
        /// <summary>Number of collision components in this steam frame.</summary>
        internal int Length => self.Count;
    }
}
