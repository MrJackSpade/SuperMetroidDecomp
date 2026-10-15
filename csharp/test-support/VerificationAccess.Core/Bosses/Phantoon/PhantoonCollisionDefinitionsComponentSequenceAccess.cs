
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification access to <see cref="PhantoonCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class PhantoonCollisionDefinitionsComponentSequenceAccess
{
    /// <summary>Exposes the collision-component count in a Phantoon frame.</summary>
    extension(PhantoonCollisionDefinitions.ComponentSequence self)
    {
        /// <summary>Number of components in this Phantoon collision frame.</summary>
        internal int Length => self.Count;
    }
}
