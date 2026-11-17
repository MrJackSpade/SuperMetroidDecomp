namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Verification access to <see cref="MenuMissileAnimation"/> members production does not use.</summary>
internal static class MenuMissileAnimationAccess
{
    /// <summary>Exposes <see cref="MenuMissileAnimation"/> state that only verification reads.</summary>
    extension(MenuMissileAnimation self)
    {
        /// <summary>The updates the shown picture has left before the roll moves on.</summary>
        internal int UpdatesLeft => self.player.UpdatesLeft;
    }
}
