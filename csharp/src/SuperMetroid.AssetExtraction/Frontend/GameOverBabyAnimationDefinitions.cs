using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Cartridge identities of the game-over Baby program's records, read only by the extraction pipeline and its
/// ROM-oracle verification. The simulation runs <see cref="GameOverBabyAnimation"/> by step ordinal and never
/// reads these values.
/// </summary>
/// <remarks>
/// Native $82:BC27..BD96 holds sixty six-byte frame records in play order. The first cry step of each group is
/// followed by a two-byte sound callback, and the final record by the $FFFF restart marker at $BD95.
/// </remarks>
internal static class GameOverBabyAnimationDefinitions
{
    /// <summary>First native frame record at <c>$82:BC27</c>, where extraction begins its walk of the stream.</summary>
    public const ushort FirstPointer = 0xbc27;

    /// <summary>Native menu-spritemap identity used only for extraction parity.</summary>
    /// <remarks>
    /// Issues #625 and #971: for semantic frame f=0..2 (Closed, Middle,
    /// Open), native spritemap ID is $65+f. A pinned NTSC J/U v1.0 ROM walk of
    /// the complete $82:BC27..BD95 Baby instruction stream found 60 positive
    /// frame records, using IDs $65/$66/$67 exactly 15/30/15 times, with three
    /// separate cry words. Menu spritemap pointer-table entries $82:C633,
    /// C635, and C637 resolve to $82:CFF6, CFFD, and D004 as bank_82.asm says.
    /// The enum domain is only these three frames; unknown values fail.
    /// </remarks>
    public static ushort NativeSpritemap(GameOverBabyFrame frame) => frame switch
    {
        GameOverBabyFrame.Closed => 0x65,
        GameOverBabyFrame.Middle => 0x66,
        GameOverBabyFrame.Open => 0x67,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Native bank-$82 palette pointer used only for extraction parity.</summary>
    /// <remarks>
    /// Issues #625 and #972: for semantic palette phase p=0..3 (Idle,
    /// ClosedCry, MiddleCry, OpenCry), native source is $82:BD97+$20*p.
    /// Each contiguous block has 16 little-endian color words; four blocks
    /// occupy $82:BD97..BE16. A pinned NTSC J/U v1.0 ROM walk of all 60
    /// positive Baby frame records found phase counts 36, 6, 12, and 6,
    /// with no pointer outside these four sources. bank_82.asm names the same
    /// blocks. Unknown semantic phases fail rather than extrapolate.
    /// </remarks>
    public static ushort NativePalettePointer(GameOverBabyPalette palette) => palette switch
    {
        GameOverBabyPalette.Idle => 0xbd97,
        GameOverBabyPalette.ClosedCry => 0xbdb7,
        GameOverBabyPalette.MiddleCry => 0xbdd7,
        GameOverBabyPalette.OpenCry => 0xbdf7,
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };

    /// <summary>
    /// The bank-$82 cry callback each cry marker names (#1416: the Core enum that held these words is deleted):
    /// <c>Instruction_Queue_BabyMetroid_Cry1_SoundEffect</c> to <c>Cry3</c> at $82:BC0C, BC15 and BC1E, which queue
    /// library-three sounds $23, $26 and $27.
    /// </summary>
    /// <remarks>Issues #625 and #970: their effect IDs and stream references were checked against pinned NTSC J/U v1.0
    /// ROM. <see cref="GameOverBabySound.None"/> has no callback.</remarks>
    internal static readonly NativeWordIdentities<GameOverBabySound> CryCallbacks = new(new Dictionary<GameOverBabySound, ushort>
    {
        [GameOverBabySound.FirstCry] = 0xbc0c,
        [GameOverBabySound.SecondCry] = 0xbc15,
        [GameOverBabySound.ThirdCry] = 0xbc1e,
    });

    /// <summary>Native bank-$82 sound callback identity used only for extraction parity.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sound"/> queues no cry.</exception>
    public static ushort NativeSoundOpcode(GameOverBabySound sound) => CryCallbacks.Word(sound);

}
