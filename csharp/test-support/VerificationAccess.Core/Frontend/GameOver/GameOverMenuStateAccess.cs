namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access to <see cref="GameOverMenuState"/> members production does not use.</summary>
internal static class GameOverMenuStateAccess
{
    /// <summary>Reads the game-over Baby's displayed pose and program step.</summary>
    extension(GameOverMenuState self)
    {
        /// <summary>Currently displayed Baby Metroid pose; the first step's pose before the program starts.</summary>
        internal GameOverBabyFrame BabyFrame => GameOverBabyAnimation.Get(self.BabyStep ?? 0).Frame;

        /// <summary>Current step of the Baby pose program, or null before its first animation update.</summary>
        internal int? BabyStep => self.BabyStep;
    }
}
