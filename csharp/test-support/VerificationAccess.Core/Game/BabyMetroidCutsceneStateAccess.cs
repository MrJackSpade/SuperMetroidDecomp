using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="BabyMetroidCutsceneState"/> members production does not use.</summary>
internal static class BabyMetroidCutsceneStateAccess
{
    /// <summary>Reads the Baby's invisible enemy-property bit without exposing mutable cutscene state.</summary>
    extension(BabyMetroidCutsceneState self)
    {
        /// <summary>True when enemy property `$0100` suppresses the Baby's spritemap.</summary>
        internal bool IsInvisible => self.Properties.HasAny(EnemyProperties.Invisible);
    }
}
