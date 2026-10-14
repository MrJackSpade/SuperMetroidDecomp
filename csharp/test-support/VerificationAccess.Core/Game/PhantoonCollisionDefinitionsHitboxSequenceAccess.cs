using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="PhantoonCollisionDefinitions.HitboxSequence"/> members production does not use.</summary>
internal static class PhantoonCollisionDefinitionsHitboxSequenceAccess
{
    /// <summary>Exposes the number of contact hitboxes in a Phantoon collision list.</summary>
    extension(PhantoonCollisionDefinitions.HitboxSequence self)
    {
        /// <summary>Number of hitboxes in this Phantoon collision list.</summary>
        internal int Length => self.Count;
    }
}
