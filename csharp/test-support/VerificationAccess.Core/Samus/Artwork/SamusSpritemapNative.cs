using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's reading of the cartridge's bank-$92 pose-half spritemap bases, which recorded native memory names
/// Samus's drawn body halves by: the retail cartridge's tables, read here, never the import's decoded body.
/// </summary>
internal static class SamusSpritemapNative
{
    /// <summary>The retail cartridge, read once.</summary>
    private static readonly Lazy<IImportCartridgeSource> cartridge = new(() =>
        (CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>The cartridge's first spritemap table entry of <paramref name="pose"/>'s <paramref name="half"/>.</summary>
    internal static int Base(SamusPoseId pose, SamusBodyHalf half) =>
        cartridge.Value.ReadCartridgeWord((half == SamusBodyHalf.Top ? SamusSourceDefinitions.SpritemapTopBases.Value
            : SamusSourceDefinitions.SpritemapBottomBases.Value) + SamusPoseNumbers.Number(pose) * 2);

    /// <summary>
    /// The cartridge's spritemap index for a drawn body picture: its pose half's first table entry plus its animation
    /// frame, or zero for no picture, the word the cartridge's bottom selector clears when a frame draws no bottom half.
    /// </summary>
    internal static int Index(SamusBodyHalfFrame? frame) =>
        frame is { } drawn ? Base(drawn.Pose, drawn.Half) + drawn.AnimationFrame : 0;

    /// <summary>
    /// The body picture a recorded cartridge spritemap index names: the frame of <paramref name="pose"/>'s half that reads
    /// it, or, for an index before that half's first entry (drawn under an earlier pose), the nearest pose half whose
    /// decoded frames read it, which shows the same composition. A zero bottom index is no picture.
    /// </summary>
    /// <exception cref="InvalidDataException">No pose half's decoded frames read the index.</exception>
    internal static SamusBodyHalfFrame? Frame(SamusSpritemapSources spritemaps, SamusPoseId pose, SamusBodyHalf half, int index)
    {
        Ensure.NotNull(spritemaps);
        if (half == SamusBodyHalf.Bottom && index == 0)
            return null;
        IEnumerable<SamusPoseId> candidates = [pose, .. SamusPoseNumbers.RecordedPoses.OrderByDescending(other => Base(other, half))];
        foreach (SamusPoseId candidate in candidates)
        {
            var frame = new SamusBodyHalfFrame(candidate, half, index - Base(candidate, half));
            if (frame.AnimationFrame >= 0 && spritemaps.Covers(frame))
                return frame;
        }
        throw new InvalidDataException($"No Samus pose half's frames read spritemap index {index}.");
    }
}
