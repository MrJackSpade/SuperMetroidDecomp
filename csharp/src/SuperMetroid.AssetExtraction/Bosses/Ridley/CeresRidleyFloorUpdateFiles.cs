using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>
/// Reads where the getaway's floor runs take their tile numbers from out of the cartridge's floor update lists, and
/// reads and writes the installed document that carries them (#1415).
/// </summary>
public static class CeresRidleyFloorUpdateFiles
{
    /// <summary>Reads each capsule and wing run's tile source from its descriptor and serializes the validated document.</summary>
    /// <param name="bus">Supported-cartridge address space holding the getaway's floor update lists.</param>
    /// <returns>UTF-8 JSON bytes for the versioned floor update document.</returns>
    /// <exception cref="InvalidDataException">A run takes its tiles from outside its list's bank.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        return Write(new CeresRidleyFloorUpdateDocument
        {
            Version = CeresRidleyFloorUpdateFormat.Version,
            CapsuleFrames = Sources(cartridge, CeresSourceDefinitions.FloorAnimation.CapsuleFrames, CeresRidleyFloorUpdateSources.CapsuleRunCount),
            WingFrames = Sources(cartridge, CeresSourceDefinitions.FloorAnimation.WingFrames, CeresRidleyFloorUpdateSources.WingRunCount),
        });
    }

    /// <summary>Loads the installed sources, rejecting unknown properties, another version or a misshapen frame.</summary>
    /// <param name="json">Caller-owned stream holding the floor update document.</param>
    /// <returns>The validated sources.</returns>
    /// <exception cref="InvalidDataException">The document's schema, version or shape is invalid.</exception>
    public static CeresRidleyFloorUpdateSources Read(Stream json)
    {
        Ensure.NotNull(json);
        CeresRidleyFloorUpdateDocument document =
            JsonAssetDocument.Read<CeresRidleyFloorUpdateDocument>(json, ArtworkJson.Options, "Ceres Ridley floor update");
        if (document.Version != CeresRidleyFloorUpdateFormat.Version)
            throw new InvalidDataException($"Ceres Ridley floor updates require version {CeresRidleyFloorUpdateFormat.Version}.");
        return CeresRidleyFloorUpdateSources.Load(document.CapsuleFrames, document.WingFrames);
    }

    /// <summary>Serializes the document as UTF-8 JSON and validates it through <see cref="Read"/> before returning it.</summary>
    /// <param name="document">The complete floor update document.</param>
    /// <returns>Validated JSON bytes, without writing a file.</returns>
    public static byte[] Write(CeresRidleyFloorUpdateDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Each of <paramref name="frames"/>' first <paramref name="runCount"/> run sources, by frame then run.</summary>
    private static int[][] Sources(IImportCartridgeSource cartridge, SnesAddress[] frames, int runCount) =>
        [.. frames.Select(list => Enumerable.Range(0, runCount).Select(run => Source(cartridge, list, run)).ToArray())];

    /// <summary>The tile source descriptor <paramref name="run"/> of <paramref name="list"/> names, within the list's own bank.</summary>
    /// <exception cref="InvalidDataException">The descriptor names a source in another bank, which the floor update residue cannot record.</exception>
    private static int Source(IImportCartridgeSource cartridge, SnesAddress list, int run)
    {
        SnesAddress source = SnesAddress.FromBusAddress(RomDataReader.ReadLongFixedBank(cartridge,
            list.AddWithinBank(run * CeresSourceDefinitions.FloorAnimation.DescriptorBytes + CeresSourceDefinitions.FloorAnimation.DescriptorSource)));
        if (source.Bank != list.Bank)
            throw new InvalidDataException($"Ceres floor update list {list} run {run} takes its tiles from {source}, outside the list's bank.");
        return source.Offset;
    }
}
