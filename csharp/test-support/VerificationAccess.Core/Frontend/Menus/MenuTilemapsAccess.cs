namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Verification access to <see cref="MenuTilemaps"/> members production does not use.</summary>
internal static class MenuTilemapsAccess
{
    /// <summary>Exposes <see cref="MenuTilemaps"/> state that only verification reads.</summary>
    extension(MenuTilemaps self)
    {
        /// <summary>All four screens, for the capture.</summary>
        internal ReadOnlySpan<ushort> All => self.words;
    }
}
