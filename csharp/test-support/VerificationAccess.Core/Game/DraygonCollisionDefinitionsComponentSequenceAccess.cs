using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="DraygonCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class DraygonCollisionDefinitionsComponentSequenceAccess
{
    /// <summary>Reports whether a Draygon frame supplies its single collision component.</summary>
    extension(DraygonCollisionDefinitions.ComponentSequence self)
    {
        /// <summary>Zero or one collision component depending on this frame's hitbox presence.</summary>
        internal int Length => self.HasComponent ? 1 : 0;
    }
}
