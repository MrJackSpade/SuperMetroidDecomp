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
        /// Runs the menu's opening as the game runs it, up to the update on which it first takes an answer: the black hold
        /// and the set-up, each with the display frame it waits out, the music wait once <paramref name="audio"/> has
        /// played the set-up's music out, and the fade-in, whose update that reaches full brightness opens the answers.
        /// </summary>
        /// <param name="audio">The menu's audio queues, which the check plays out as the game's audio would.</param>
        internal void RunOpening(SuperMetroid.Core.Audio.AudioQueues audio)
        {
            Ensure.NotNull(audio);
            for (int update = 0; update < 4; update++)
                self.Step(0);
            while (audio.HasQueuedMusic)
                _ = audio.AdvanceFrame(default);
            self.Step(0);
            while (!self.ComposeScene().Brightness.IsFull)
                self.Step(0);
        }
    }
}
