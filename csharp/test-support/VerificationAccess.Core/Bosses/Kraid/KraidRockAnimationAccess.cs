using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidRockAnimation"/> members production does not use.</summary>
internal static class KraidRockAnimationAccess
{
    /// <summary>Exposes <see cref="KraidRockAnimation"/> state that only verification reads.</summary>
    extension(KraidRockAnimation self)
    {
        /// <summary>The program played.</summary>
        internal KraidRockProgram Program => self.Cursor.Program;
    }
}
