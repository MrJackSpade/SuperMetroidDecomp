namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access to <see cref="GameOverMenuState"/> members production does not use.</summary>
internal static class GameOverMenuStateAccess
{
    /// <summary>Reads the game-over Baby's displayed pose.</summary>
    extension(GameOverMenuState self)
    {
        /// <summary>Currently displayed Baby Metroid pose; its resting pose before the animation starts.</summary>
        internal GameOverBabyFrame BabyFrame => self.BabyLook.Frame;

        /// <summary>
        /// Puts the menu at <paramref name="phase"/> with no display frame being waited out, for a check that starts at
        /// that step without running the updates that reach it.
        /// </summary>
        internal void SkipToPhase(GameOverMenuPhase phase)
        {
            self.IsWaitingOutDisplayFrame = false;
            self.Phase = phase;
        }
    }
}
