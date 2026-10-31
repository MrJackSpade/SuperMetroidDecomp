using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Compatibility;
using static SuperMetroid.AssetExtraction.CartridgeTables.CeresSourceDefinitions.FloorAnimation;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>
/// Decodes the Speed Booster delays the getaway's wing frames leave behind (#1338, #1479), and reads and writes the
/// installed document that carries them. The unauthored cadence's stream word names a place in the floor update queue;
/// the getaway queues a capsule frame's descriptors and then a wing frame's there, so the delays are the bytes of the
/// wing's list that land where the cadence reads. The capsule's descriptors, the zero byte after them and the cleared
/// first word all end before that place, so a capsule-only frame leaves the delays as they were and each wing frame's
/// delays depend on that frame alone.
/// </summary>
public static class CeresRidleyGetawayBoostDelayFiles
{
    /// <summary>The value a list's control byte holds after its last descriptor.</summary>
    private const int ListEnd = 0;

    /// <summary>Decodes each wing frame's delays and serializes the validated document.</summary>
    /// <param name="bus">Supported-cartridge address space holding the getaway's floor update lists and Samus's running cadences.</param>
    /// <returns>UTF-8 JSON bytes for the versioned delay document.</returns>
    /// <exception cref="InvalidDataException">
    /// The unauthored cadence does not read the floor update queue, a list does not hold the descriptors the getaway
    /// queues, or the cadence reads bytes the wing's descriptors do not write.
    /// </exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        foreach (SnesAddress capsule in CapsuleFrames)
            RequireRuns(cartridge, capsule, CapsuleRunCount);
        int capsuleBytes = CapsuleRunCount * DescriptorBytes;
        int wingStart = FirstQueueByte(cartridge) - capsuleBytes;
        // The capsule's list and the zero written after it end before the cadence's first byte, and its last byte lies
        // within the wing's descriptors.
        if (wingStart <= 0 || wingStart + UnauthoredBoostCadence.FrameCount > WingRunCount * DescriptorBytes)
            throw new InvalidDataException("The unauthored Speed Booster cadence does not read the getaway wing's descriptors alone.");
        return Write(new CeresRidleyGetawayBoostDelayDocument
        {
            Version = CeresRidleyGetawayBoostDelayFormat.Version,
            WingFrames = [.. WingFrames.Select(wing =>
            {
                RequireRuns(cartridge, wing, WingRunCount);
                return Enumerable.Range(0, UnauthoredBoostCadence.FrameCount)
                    .Select(frame => (int)cartridge.ReadCartridgeByte((int)wing.AddWithinBank(wingStart + frame))).ToArray();
            })],
        });
    }

    /// <summary>Loads the installed delays, rejecting unknown properties, another version or a misshapen frame.</summary>
    /// <param name="json">Caller-owned stream holding the delay document.</param>
    /// <returns>The validated delays.</returns>
    /// <exception cref="InvalidDataException">The document's schema, version or shape is invalid.</exception>
    public static CeresRidleyGetawayBoostDelays Read(Stream json)
    {
        Ensure.NotNull(json);
        CeresRidleyGetawayBoostDelayDocument document =
            JsonAssetDocument.Read<CeresRidleyGetawayBoostDelayDocument>(json, ArtworkJson.Options, "Ceres Ridley getaway boost delay");
        if (document.Version != CeresRidleyGetawayBoostDelayFormat.Version)
            throw new InvalidDataException($"Ceres Ridley getaway boost delays require version {CeresRidleyGetawayBoostDelayFormat.Version}.");
        return CeresRidleyGetawayBoostDelays.Load(Frames(document.WingFrames));
    }

    /// <summary>The document's wing frames, which a malformed document can leave out.</summary>
    /// <exception cref="InvalidDataException">The document names no wing frames.</exception>
    private static int[][] Frames(int[][]? frames) =>
        frames ?? throw new InvalidDataException("Ceres Ridley getaway boost delays name no wing frames.");

    /// <summary>Serializes the document as UTF-8 JSON and validates it through <see cref="Read"/> before returning it.</summary>
    /// <param name="document">The complete delay document.</param>
    /// <returns>Validated JSON bytes, without writing a file.</returns>
    public static byte[] Write(CeresRidleyGetawayBoostDelayDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>
    /// Where in the floor update queue the unauthored cadence's first delay lies: its stream word names a bank-$91 place
    /// below the cartridge half, which shows the queue.
    /// </summary>
    /// <exception cref="InvalidDataException">The stream word names no place whose ten delays lie in the queue.</exception>
    private static int FirstQueueByte(IImportCartridgeSource cartridge)
    {
        int position = RomDataReader.ReadWordFixedBank(cartridge, SamusSourceDefinitions.UnauthoredBoostStreamWord);
        int queueByte = position - QueueStart;
        if (position >= SamusSourceDefinitions.CartridgeWindowStart || queueByte < 0 || queueByte + UnauthoredBoostCadence.FrameCount > QueueBytes)
            throw new InvalidDataException($"The unauthored Speed Booster stream word ${position:X4} does not name the floor update queue.");
        return queueByte;
    }

    /// <summary>Requires <paramref name="list"/> to hold exactly <paramref name="runs"/> descriptors before its end.</summary>
    /// <exception cref="InvalidDataException">The list ends early or holds more.</exception>
    private static void RequireRuns(IImportCartridgeSource cartridge, SnesAddress list, int runs)
    {
        for (int run = 0; run < runs; run++)
        {
            if (cartridge.ReadCartridgeByte((int)list.AddWithinBank(run * DescriptorBytes)) == ListEnd)
                throw new InvalidDataException($"Ceres floor update list {list} ends after {run} of its {runs} descriptors.");
        }
        if (cartridge.ReadCartridgeByte((int)list.AddWithinBank(runs * DescriptorBytes)) != ListEnd)
            throw new InvalidDataException($"Ceres floor update list {list} holds more than {runs} descriptors.");
    }
}
