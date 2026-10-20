namespace SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Programs;

/// <summary>Verification access to <see cref="GameOverRomData.BabyAnimation"/> members production does not use.</summary>
internal static class GameOverRomDataBabyAnimationAccess
{
    /// <summary>Maps the game-over Baby's native cry opcodes to sound identities.</summary>
    extension(GameOverRomData.BabyAnimation)
    {
        /// <summary>Maps a bank-$82 animation opcode to its library-qualified cry.</summary>
        /// <remarks>
        /// Issues #625 and #970: pinned NTSC J/U v1.0 ROM and bank_82.asm match
        /// all three native instructions at $82:BC0C+9*i for i=0..2. Each loads
        /// library-3 effect $23, $26, or $27 and calls the same sound queue.
        /// The game-over Baby stream references those opcodes once each at
        /// $82:BC5D, BCEF, and BD69. Opcode spacing is regular, but the effect
        /// identities select three distinct sound operations; use the named cases and
        /// reject unknown opcodes.
        /// </remarks>
        internal static SoundEffectId ResolveCry(ushort opcode) =>
            ClosedNativeWords.Decode<GameOverBabyCryInstruction>(opcode, "game-over Baby instruction") switch
            {
                GameOverBabyCryInstruction.Cry23 => GameOverRomData.BabyAnimation.Cry23,
                GameOverBabyCryInstruction.Cry26 => GameOverRomData.BabyAnimation.Cry26,
                GameOverBabyCryInstruction.Cry27 => GameOverRomData.BabyAnimation.Cry27,
                var undefined => throw new InvalidOperationException($"Undefined GameOverBabyCryInstruction {undefined}."),
            };
    }
}
