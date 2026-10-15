using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Immutable RGB5 artwork for Zebetite's eight-frame two-color pulse.</summary>
public sealed class ZebetiteColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("ZebetiteColorCatalog-v1", content =>
        {
            content.Append("frames", ZebetiteColorFormat.FrameCount);
            Span<Bgr555> row = stackalloc Bgr555[ZebetiteColorFormat.ColorsPerFrame];
            for (int frame = 0; frame < ZebetiteColorFormat.FrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Only palette cells edited away from the calculated native pulse, keyed by frame and color.</summary>
    internal readonly Dictionary<int, Bgr555> edits;

    /// <summary>Captures validated color overrides for an eight-frame barrier pulse.</summary>
    private ZebetiteColorCatalog(Dictionary<int, Bgr555> edits) => this.edits = edits;

    /// <summary>Calculated symmetric pulse of the two selected barrier-core paints.</summary>
    private static Bgr555 NativeColor(int frame, int color) => ZebetitePulsePaintDefinitions.Color(frame, color);

    /// <summary>Selects an edited pulse color when present, otherwise the symmetric native color.</summary>
    private Bgr555 Resolve(int frame, int color) =>
        edits.TryGetValue(frame * ZebetiteColorFormat.ColorsPerFrame + color, out Bgr555 edited)
            ? edited : NativeColor(frame, color);

    /// <summary>Strict camel-case JSON options shared by load and write.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads all eight two-color pulse rows, rejecting duplicate or unknown properties, unsupported versions, incorrect dimensions, and RGB5 channels outside 0..31; preserves each supplied color edit independently of the stock symmetric pulse.</summary>
    /// <param name="json">UTF-8 JSON stream containing the versioned Zebetite color document.</param>
    /// <returns>The validated immutable barrier-color catalog.</returns>
    public static ZebetiteColorCatalog Load(Stream json)
    {
        ZebetiteColorDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ZebetiteColorDocument>(Options)
                ?? throw new InvalidDataException("Zebetite colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Zebetite color JSON.", error);
        }
        if (document.Version != ZebetiteColorFormat.Version ||
            !ZebetiteColorFormat.Shape.HasRows(document.Frames))
            throw new InvalidDataException("Zebetite colors require all eight frames at the supported version.");

        var edits = new Dictionary<int, Bgr555>();
        for (int frame = 0; frame < document.Frames.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (!ZebetiteColorFormat.Shape.HasColumns(colors))
                throw new InvalidDataException($"Zebetite frame {frame} requires two RGB5 colors.");
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Zebetite frame {frame}, color {color} requires RGB5 channels.");
                Bgr555 selected = rgb.ToBgr555();
                if (selected != NativeColor(frame, color))
                    edits.Add(frame * ZebetiteColorFormat.ColorsPerFrame + color, selected);
            }
        }
        return new(edits);
    }

    /// <summary>Writes the selected pulse row's two colors without advancing the shared palette counter or testing fade/linkage conditions; native <c>HandleZebetitePaletteAnimation</c> at <c>$A6:FD5E</c> targets CGRAM entries 172 and 173 (OBJ palette 2 colors 12 and 13).</summary>
    /// <param name="cgram">Destination color memory to update.</param>
    /// <param name="frame">Zero-based pulse row from 0 through 7.</param>
    /// <param name="destinationColor">CGRAM color-entry index for the first color, not a byte offset; the next entry receives the second color.</param>
    [PpuLayoutBoundary("codec: palette-cycle colours at their native CGRAM index operand")]
    public void Apply(SnesCgram cgram, int frame, int destinationColor)
    {
        Ensure.NotNull(cgram);
        if ((uint)frame >= ZebetiteColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < ZebetiteColorFormat.ColorsPerFrame; color++)
            new CgramPalettes(cgram).All.Write(destinationColor + color, Resolve(frame, color));
    }

    /// <summary>Serializes a Zebetite color document as indented camel-case UTF-8 JSON, then validates the result through <see cref="Load"/> before returning it.</summary>
    /// <param name="document">Document containing the supported version and all eight two-color RGB5 rows.</param>
    /// <returns>Validated JSON bytes suitable for the barrier-color asset file.</returns>
    public static byte[] Write(ZebetiteColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Rejects repeated property names before deserialization can overwrite an earlier value.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Zebetite color property."));
}
