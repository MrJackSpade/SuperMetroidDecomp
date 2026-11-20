using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Cartridge identities of the game-over Baby program's records, read only by the extraction pipeline and its
/// ROM-oracle verification. The simulation plays <see cref="GameOverBabyAnimation"/> and never reads these values.
/// </summary>
/// <remarks>
/// Native $82:BC27..BD96 holds sixty six-byte frame records in play order. The first cry step of each group is
/// followed by a two-byte sound callback, and the final record by the $FFFF restart marker at $BD95.
/// </remarks>
[EncodingOwner("cartridge record words of the game-over Baby program")]
internal static class GameOverBabyAnimationDefinitions
{
    /// <summary>First native frame record at <c>$82:BC27</c>, where extraction begins its walk of the stream.</summary>
    public const ushort FirstPointer = 0xbc27;

    /// <summary>Each frame's native menu-spritemap identity, used only for extraction parity.</summary>
    /// <remarks>
    /// The Closed, Middle and Open frames draw the menu spritemaps that follow one another in the menu spritemap table,
    /// in that order. The table holds only these three frames.
    /// </remarks>
    internal static readonly NativeWordIdentities<GameOverBabyFrame> Spritemaps = new(new Dictionary<GameOverBabyFrame, ushort>
    {
        [GameOverBabyFrame.Closed] = 0x65,
        [GameOverBabyFrame.Middle] = 0x66,
        [GameOverBabyFrame.Open] = 0x67,
    });

    /// <summary>Each colouring's native bank-$82 palette pointer, used only for extraction parity.</summary>
    /// <remarks>
    /// The Idle, ClosedCry, MiddleCry and OpenCry colourings lie in consecutive blocks of sixteen colours, in that order.
    /// The table holds only these four sources.
    /// </remarks>
    internal static readonly NativeWordIdentities<GameOverBabyColouring> PalettePointers = new(new Dictionary<GameOverBabyColouring, ushort>
    {
        [GameOverBabyColouring.Idle] = 0xbd97,
        [GameOverBabyColouring.ClosedCry] = 0xbdb7,
        [GameOverBabyColouring.MiddleCry] = 0xbdd7,
        [GameOverBabyColouring.OpenCry] = 0xbdf7,
    });

    /// <summary>
    /// The cry callback word each cry marker names: the first, second and third cry each queue their own sound.
    /// </summary>
    internal static readonly NativeWordIdentities<GameOverBabySound> CryCallbacks = new(new Dictionary<GameOverBabySound, ushort>
    {
        [GameOverBabySound.HighCry] = 0xbc0c,
        [GameOverBabySound.HitchedCry] = 0xbc15,
        [GameOverBabySound.LowCry] = 0xbc1e,
    });
}
