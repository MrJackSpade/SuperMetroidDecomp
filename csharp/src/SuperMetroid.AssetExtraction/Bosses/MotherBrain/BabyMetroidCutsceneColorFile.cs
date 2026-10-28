using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed cutscene Baby Metroid colour document.</summary>
public static class BabyMetroidCutsceneColorFile
{
    /// <summary>Compiles initial and fade RGB5 artwork from installed JSON, retaining edits independently of the calculated native color compositions.</summary>
    /// <param name="json">UTF-8 JSON input read from its current position; the stream remains open.</param>
    /// <returns>A catalog whose private compiled images are independent of the source document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, contains duplicate or unknown properties, uses an unsupported version, or lacks fifteen initial colors and six fourteen-color fade rows with RGB channels from zero through 31.</exception>
    public static BabyMetroidCutsceneColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        BabyMetroidCutsceneColorDocument document;
        document = JsonAssetDocument.Read<BabyMetroidCutsceneColorDocument>(json, ArtworkJson.Options, "cutscene Baby color");
        if (document.Version != BabyMetroidCutsceneColorFormat.Version)
            throw new InvalidDataException("Cutscene Baby colors require the supported version.");
        if (document.Fade is null ||
            document.Fade.Length != BabyMetroidCutsceneColorDefinitions.FadeFrameCount)
            throw new InvalidDataException("Cutscene Baby fade requires six displayed frames.");
        return new(Compile(document.Initial,
                BabyMetroidCutsceneColorDefinitions.InitialColorCount, "initial"),
            document.Fade.Select((frame, index) => Compile(frame,
                BabyMetroidCutsceneColorDefinitions.FadeColorCount,
                $"fade frame {index + 1}")).ToArray());
    }

    /// <summary>Serializes editable artwork as indented, camel-case UTF-8 JSON, then validates the result using the installed-content loader.</summary>
    /// <param name="document">Initial and ordered fade images to serialize; the supplied arrays are not modified.</param>
    /// <returns>A newly allocated JSON byte array for <see cref="BabyMetroidCutsceneColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document is null or fails schema, image-dimension, or RGB5 channel validation.</exception>
    public static byte[] Write(BabyMetroidCutsceneColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Validates an RGB5 row's length and channels before copying it to packed colors.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int required, string name) =>
        Rgb5Rows.Compile(source, required, $"Cutscene Baby {name}");
}
