namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="FakeKraidProjectileAnimation"/> members production does not use.</summary>
internal static class FakeKraidProjectileAnimationAccess
{
    /// <summary>Exposes <see cref="FakeKraidProjectileAnimation"/> state that only verification reads.</summary>
    extension(FakeKraidProjectileAnimation self)
    {
        /// <summary>The pose shown.</summary>
        internal FakeKraidProjectilePose Pose => self.Cursor.Program;
    }
}
