using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec of the cartridge's Samus running cadences (#1473): a word naming the ordinary stream, eleven-byte streams of
/// ten frame delays closed by the loop byte, five words naming the boost stages' streams with the stage-five word past
/// them, and the boost restart words. The game holds typed delay lists; this is the one place their layout is known,
/// for checks.
/// </summary>
[EncodingOwner("cartridge Samus running cadences")]
internal static class SamusRunningCadenceCodec
{
    /// <summary>The bank holding the cadences; each stream word names a place in it.</summary>
    private const int Bank = 0x910000;

    /// <summary>The word naming the ordinary running stream.</summary>
    private const int OrdinaryStreamWord = 0x91b5d1;

    /// <summary>The words naming each boost stage's stream, then the stage-five word.</summary>
    private const int BoostStreamWords = 0x91b5de;

    /// <summary>The boost restart words, one per selection.</summary>
    private const int RestartWords = 0x91b61f;

    /// <summary>The bank-$91 window position below which a stream word names memory, not cartridge data.</summary>
    private const int CartridgeWindowStart = 0x8000;

    /// <summary>The bank's low-half position the floor update residue starts at.</summary>
    private const int ResidueWindowStart = 0x02d0;

    /// <summary>The byte that closes each stream: the loop back to frame zero.</summary>
    private const byte LoopByte = 0xff;

    /// <summary>The frames each stream times.</summary>
    internal const int FrameCount = 10;

    /// <summary>The selection past the authored stages, which a full sound queue reports.</summary>
    internal const int UnauthoredSelection = 5;

    /// <summary>The ordinary running stream's ten delays.</summary>
    internal static IReadOnlyList<int> OrdinaryDelays(IImportCartridgeSource cartridge) =>
        Stream(cartridge, cartridge.ReadCartridgeWord(OrdinaryStreamWord));

    /// <summary>Boost stage <paramref name="stage"/>'s ten delays, zero through four.</summary>
    internal static IReadOnlyList<int> BoostDelays(IImportCartridgeSource cartridge, int stage) =>
        Stream(cartridge, cartridge.ReadCartridgeWord(BoostStreamWords + Ensure.BetweenInclusive(0, 4, stage) * sizeof(ushort)));

    /// <summary>The restart countdown selection <paramref name="selection"/> reads, zero through five.</summary>
    internal static int CountdownAfterAdvance(IImportCartridgeSource cartridge, int selection) =>
        cartridge.ReadCartridgeWord(RestartWords + Ensure.BetweenInclusive(0, UnauthoredSelection, selection) * sizeof(ushort));

    /// <summary>
    /// The floor update residue byte the unauthored selection's first frame reads: its stream word names a place in the
    /// bank's low half, which aliases the residue.
    /// </summary>
    internal static int UnauthoredFirstResidueByte(IImportCartridgeSource cartridge)
    {
        int position = cartridge.ReadCartridgeWord(BoostStreamWords + UnauthoredSelection * sizeof(ushort));
        if (position is >= CartridgeWindowStart or < ResidueWindowStart)
            throw new InvalidDataException($"The unauthored stage's stream word ${position:X4} does not name the floor update residue.");
        return position - ResidueWindowStart;
    }

    /// <summary>The ten delays of the stream at bank-$91 window position <paramref name="position"/>, checking its closing loop byte.</summary>
    private static int[] Stream(IImportCartridgeSource cartridge, int position)
    {
        var delays = new int[FrameCount];
        for (int frame = 0; frame < FrameCount; frame++)
            delays[frame] = cartridge.ReadCartridgeByte(Bank | (position + frame));
        if (cartridge.ReadCartridgeByte(Bank | (position + FrameCount)) != LoopByte)
            throw new InvalidDataException($"The running stream at $91:{position:X4} is not closed by its loop byte.");
        return delays;
    }
}
