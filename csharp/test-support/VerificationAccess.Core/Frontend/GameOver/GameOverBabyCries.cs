using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification's reading of the game-over Baby's cartridge cry callbacks.</summary>
internal static class GameOverBabyCries
{
    /// <summary>
    /// The cry a bank-$82 cry callback queues: the import's callback table names the marker, and the menu's own mapping
    /// names its sound. An unknown callback fails loudly.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #970: pinned NTSC J/U v1.0 ROM and bank_82.asm match all three native instructions at
    /// $82:BC0C+9*i for i=0..2, each loading library-3 effect $23, $26 or $27; the game-over Baby stream references
    /// them once each at $82:BC5D, BCEF and BD69.
    /// </remarks>
    /// <exception cref="InvalidDataException"><paramref name="opcode"/> is not a cry callback.</exception>
    internal static SoundEffectId ResolveCry(ushort opcode) =>
        GameOverMenuState.CrySound(GameOverBabyAnimationDefinitions.CryCallbacks.Decode(opcode, "game-over Baby instruction"));
}
