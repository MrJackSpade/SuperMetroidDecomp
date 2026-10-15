
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidArmHitboxSequence"/> members production does not use.</summary>
internal static class KraidArmHitboxSequenceAccess
{
    /// <summary>Materializes the ordered hitboxes for one Kraid arm frame.</summary>
    extension(KraidArmHitboxSequence self)
    {
        /// <summary>Materializes hitboxes from a Kraid arm list in native order.</summary>
        internal KraidArmCollisionHitbox[] ToArray()
        {
            var result = new KraidArmCollisionHitbox[self.Length];
            for (int index = 0; index < result.Length; index++) result[index] = self[index];
            return result;
        }
    }
}
