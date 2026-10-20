namespace SuperMetroid.Core.Hud;

/// <summary>Verification access to <see cref="GameplayMessageBoxState"/> members production does not use.</summary>
internal static class GameplayMessageBoxStateAccess
{
    /// <summary>Exposes <see cref="GameplayMessageBoxState"/> state that only verification reads.</summary>
    extension(GameplayMessageBoxState self)
    {
        /// <summary>Number of consecutive 32-tile rows in the ROM-built box.</summary>
        public int TilemapRowCount => self.Tilemap.Length / GameplayMessageRomData.Layout.TilemapWidth;
    }
}
