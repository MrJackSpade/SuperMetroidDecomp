using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Extracts what Kraid's fingernails, Crocomire's volley and the opening cinematic's egg shell read past their data,
/// decoded from the player's cartridge (#1500), and loads it as the boss and intro overread services.
/// </summary>
public static partial class BossAndIntroOverreadFiles
{
    /// <summary>
    /// Decodes every read and writes the results as JSON. No manifest is written (#1693): the document names its own format,
    /// and the cartridge it was decoded from is the installation receipt's.
    /// </summary>
    /// <param name="bus">Cartridge source for the reads.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file is replaced.</param>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = bus;
        Directory.CreateDirectory(directory);
        BossAndIntroOverreadDocument document = BossAndIntroOverreadDecoding.Decode(cartridge);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Bosses.BossAndIntroOverreadDocument>(BossAndIntroOverreadFormat.JsonOptions));
        File.WriteAllBytes(Path.Combine(directory, BossAndIntroOverreadFormat.FileName), bytes);
    }

    /// <summary>Loads the installed file, as the player may have edited it.</summary>
    /// <param name="directory">Installed overread directory holding the file.</param>
    /// <exception cref="InvalidDataException">The file's format or shape is invalid.</exception>
    public static BossAndIntroOverreadServices Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory, BossAndIntroOverreadFormat.FileName));
        return Read(new MemoryStream(bytes, writable: false));
    }

    /// <summary>Reads and validates an overread document and builds its services.</summary>
    private static BossAndIntroOverreadServices Read(Stream json) =>
        Services(JsonAssetDocument.Read<BossAndIntroOverreadDocument>(json, BossAndIntroOverreadFormat.JsonOptions, "boss and intro overread"));

    /// <summary>Builds every service from a validated document.</summary>
    /// <exception cref="InvalidDataException">The document has an unsupported version or a value out of its range.</exception>
    private static BossAndIntroOverreadServices Services(BossAndIntroOverreadDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != BossAndIntroOverreadFormat.Version)
            throw new InvalidDataException("Boss and intro overread file has an unsupported version.");
        BossAndIntroOverreadDocument.ContourRecord contour = document.KraidNailPastLedges;
        IReadOnlyList<BossAndIntroOverreadDocument.FallStep> steps = document.ShellFragmentFallPastCurve;
        if (!IsSignedWord(contour.LeftOffset) || !IsSignedWord(contour.Bound) || !IsSignedWord(document.CrocomireLastShotGradient))
            throw new InvalidDataException("Boss and intro overread file holds a word outside its range.");
        var fall = new List<SignedSixteenSixteen>();
        foreach (BossAndIntroOverreadDocument.FallStep step in steps)
            fall.Add(SignedSixteenSixteen.FromPixels(step.Pixels));
        if (fall.Count == 0)
            throw new InvalidDataException("Boss and intro overread file holds no shell fragment steps.");
        return new BossAndIntroOverreadServices(
            new KraidNailPastLedges(contour.LeftOffset, contour.Bound),
            new CrocomireLastShotGradient(document.CrocomireLastShotGradient),
            new ShellFragmentFallPastCurve(fall));
    }

    /// <summary>Whether a decoded value fits a signed word.</summary>
    private static bool IsSignedWord(int value) => value is >= short.MinValue and <= short.MaxValue;
}
