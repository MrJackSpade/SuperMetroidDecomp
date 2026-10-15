
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidFootHitboxSequence"/> members production does not use.</summary>
internal static class KraidFootHitboxSequenceAccess
{
    /// <summary>Materializes the single hitbox in a Kraid foot frame.</summary>
    extension(KraidFootHitboxSequence self)
    {
        /// <summary>Materializes the foot's single collision hitbox.</summary>
        internal KraidFootCollisionHitbox[] ToArray() => [self[0]];
    }
}
