using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

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

    private readonly Dictionary<int, Bgr555> edits;

    private ZebetiteColorCatalog(Dictionary<int, Bgr555> edits) => this.edits = edits;

    /// <summary>Calculated symmetric pulse of the two selected barrier-core paints.</summary>
    private static Bgr555 NativeColor(int frame, int color) => ZebetitePulsePaintDefinitions.Color(frame, color);

    private Bgr555 Resolve(int frame, int color) =>
        edits.TryGetValue(frame * ZebetiteColorFormat.ColorsPerFrame + color, out Bgr555 edited)
            ? edited : NativeColor(frame, color);

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
    public void Apply(SnesCgram cgram, int frame, int destinationColor)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= ZebetiteColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < ZebetiteColorFormat.ColorsPerFrame; color++)
            cgram.SetColor(destinationColor + color, Resolve(frame, color));
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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Zebetite color property."));
}

/// <summary>Editable JSON schema for Zebetite's two barrier-core inks across eight pulse steps; shared animation-counter ownership, update conditions, health, and collision remain engine behavior.</summary>
public sealed record ZebetiteColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="ZebetiteColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Eight ordered rows of two RGB5 colors, corresponding to native words at <c>$A6:FD87 + 4 * row</c>; row entries paint OBJ palette 2 colors 12 and 13 in that order.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Presentation geometry of Zebetite palette records at $A6:FD87..FDA6.</summary>
public static class ZebetiteColorFormat
{
    /// <summary>Asset filename for the editable Zebetite barrier pulse colors.</summary>
    public const string FileName = "zebetite-colors.json";
    /// <summary>Supported revision of the eight-row, two-color RGB5 JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Eight pulse rows selected by the native shared counter masked with <c>$0007</c>; these are palette steps rather than independent video-refresh timing data.</summary>
    public const int FrameCount = 8;
    /// <summary>Two adjacent barrier-core colors copied per pulse step to OBJ palette 2 colors 12 and 13.</summary>
    public const int ColorsPerFrame = 2;
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);
}
