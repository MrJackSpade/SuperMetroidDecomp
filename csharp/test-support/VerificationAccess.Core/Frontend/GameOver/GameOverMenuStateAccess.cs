namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access to <see cref="GameOverMenuState"/> members production does not use.</summary>
internal static class GameOverMenuStateAccess
{
    /// <summary>Reads the game-over Baby's displayed pose.</summary>
    extension(GameOverMenuState self)
    {
        /// <summary>Currently displayed Baby Metroid pose; its resting pose before the animation starts.</summary>
        internal GameOverBabyFrame BabyFrame => self.BabyLook.Frame;
    }
}
