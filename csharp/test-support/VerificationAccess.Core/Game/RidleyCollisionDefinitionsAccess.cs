using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="RidleyCollisionDefinitions"/> members production does not use.</summary>
internal static class RidleyCollisionDefinitionsAccess
{
    /// <summary>Enumerates Ridley's eleven side-facing frame roots and compiled contact-hitbox pointers.</summary>
    extension(RidleyCollisionDefinitions)
    {
        /// <summary>The eleven side-facing body frames, `$32` bytes apart from the leftmost.</summary>
        internal static ushort[] FramePointers
        {
            get
            {
                ushort left = (ushort)RidleyCollisionDefinitions.BodyFrame.Left;
                int stride = RidleyCollisionDefinitions.SideFrameBytes;
                return [.. Enumerable.Range(0, 11).Select(index => (ushort)(left + index * stride))];
            }
        }

        /// <summary>Native roots of Ridley's compiled contact hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => RidleyCollisionDefinitions.Lists.Pointers;
    }
}
