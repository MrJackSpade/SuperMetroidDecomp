namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification access to <see cref="CrocomireProjectileAnimation"/> members production does not use.</summary>
internal static class CrocomireProjectileAnimationAccess
{
    /// <summary>Exposes <see cref="CrocomireProjectileAnimation"/> state that only verification reads.</summary>
    extension(CrocomireProjectileAnimation self)
    {
        /// <summary>The program played.</summary>
        internal CrocomireProjectileFrameKind Kind => self.Cursor.Program;
    }
}
