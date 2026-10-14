namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access to <see cref="GameOverMenuState"/> members production does not use.</summary>
internal static class GameOverMenuStateAccess
{
    /// <summary>Reads the game-over Baby's displayed frame and instruction cursor.</summary>
    extension(GameOverMenuState self)
    {
        /// <summary>Currently displayed Baby Metroid frame, exposed for deterministic replay.</summary>
        internal ushort BabySpritemap => self.babySpritemap;

        /// <summary>Current bank-$82 Baby instruction pointer, exposed for debugger inspection.</summary>
        internal ushort BabyInstructionPointer => self.babyInstructionPointer;
    }
}
