using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable Crystal Flash body and bubble colors. Native timers, frame cursors,
/// and completion still belong to <see cref="SamusCrystalFlashState"/>.
/// </summary>
public sealed class CrystalFlashColorCatalog
{
    private readonly Bgr555[][]? body;
    private readonly Dictionary<int, Bgr555> bubble = [];

    private CrystalFlashColorCatalog(Bgr555[][] body, Bgr555[][] bubble)
    {
        // Discard stock body rows only after checking every supplied color. Edited
        // resources keep their complete, independent palette content.
        bool calculated = true;
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            calculated &= body[frame][color] == CalculateBody(frame, color);
        this.body = calculated ? null : body;
        // The rotating ramp accounts for 35 stock words. The remaining native white
        // is an independent painted color choice; retain only that supplied sample.
        // Arbitrary supplied edits remain exact.
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
            if (bubble[frame][color] != CalculateBubble(frame, color))
                this.bubble.Add(frame * CrystalFlashColorFormat.BubbleColorCount + color, bubble[frame][color]);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Returns one editable body color for a native playback record, calculating stock rows when they require no stored overrides.</summary>
    /// <param name="frame">Zero-based body record index, 0 through 9 in playback order.</param>
    /// <param name="color">Zero-based color within the ten-color body portion, including transparent color zero.</param>
    /// <returns>SNES RGB555 color word, with red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the body dimensions.</exception>
    public Bgr555 ResolveBody(int frame, int color)
    {
        if ((uint)frame >= CrystalFlashColorFormat.BodyFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= CrystalFlashColorFormat.BodyColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return body is null ? CalculateBody(frame, color) : body[frame][color];
    }

    /// <summary>
    /// $9B:96C0-$9773 body colors, selected by ten $91:DC00 records: the first
    /// frame is neutral grey16; subsequent frames pulse 19,23,27,23 twice and
    /// return to19. All nine visible body colors share that intensity. The
    /// transparent color is the fixed RGB5 backdrop (0,0,14) in every frame.
    /// </summary>
    private static Bgr555 CalculateBody(int frame, int color)
    {
        if (color == 0)
            return new Bgr555(0, 0, 14);
        int grey = frame == 0 ? 16 : 27 - 4 * Math.Abs((frame - 1) % 4 - 2);
        return new Bgr555(grey, grey, grey);
    }
    /// <summary>Returns one editable bubble color from the independently cycling six-frame sequence, preserving supplied edits and the native painted white sample.</summary>
    /// <param name="frame">Zero-based bubble palette index, 0 through 5.</param>
    /// <param name="color">Zero-based color within the six-color bubble portion, 0 through 5.</param>
    /// <returns>SNES RGB555 color word, with red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the bubble dimensions.</exception>
    public Bgr555 ResolveBubble(int frame, int color)
    {
        if ((uint)frame >= CrystalFlashColorFormat.BubbleFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= CrystalFlashColorFormat.BubbleColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return bubble.TryGetValue(frame * CrystalFlashColorFormat.BubbleColorCount + color, out Bgr555 supplied)
            ? supplied : CalculateBubble(frame, color);
    }

    /// <summary>
    /// $9B:96D4 + 32*frame rotates six pink-to-white levels one color per frame.
    /// Red stays31; green/blue descend from31 to25 with nearest-integer interpolation
    /// across five intervals. Frame5/color0 at $9B:9774 is independently painted white
    /// ($7FFF), rather than the ramp's $7BDF. The uniform five-tick traversal at
    /// $91:DBA0-DBBD and six-word copy at $91:DC88-DCAE supply no phase-specific
    /// operation generating that choice. Only that sample remains supplied stock data;
    /// a formula exception would merely reencode its frame/color identity and value.
    /// </summary>
    private static Bgr555 CalculateBubble(int frame, int color)
    {
        int phase = (color - frame + CrystalFlashColorFormat.BubbleColorCount)
            % CrystalFlashColorFormat.BubbleColorCount;
        int channel = 31 - (6 * phase + 2) / 5;
        return new Bgr555(31, channel, channel);
    }

    /// <summary>Copies one body frame to CGRAM colors $E0-$E9 without changing the bubble colors or advancing any gameplay timer.</summary>
    /// <param name="cgram">Destination color memory.</param>
    /// <param name="frame">Zero-based body playback record index, 0 through 9.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The body frame index is outside the sequence.</exception>
    public void ApplyBody(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            cgram.SetColor(SamusPaletteRomData.CrystalFlash.BodyCgramStart + color,
                ResolveBody(frame, color));
    }
    /// <summary>Copies one bubble frame to CGRAM colors $EA-$EF without changing the body colors or advancing any gameplay timer.</summary>
    /// <param name="cgram">Destination color memory.</param>
    /// <param name="frame">Zero-based bubble palette index, 0 through 5.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The bubble frame index is outside the sequence.</exception>
    public void ApplyBubble(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
            cgram.SetColor(SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color,
                ResolveBubble(frame, color));
    }

    /// <summary>Loads the supported camel-case JSON schema, rejecting duplicate or unknown properties, incorrect frame dimensions, null colors, and RGB5 channels outside 0..31.</summary>
    /// <param name="json">Readable JSON stream; ownership remains with the caller.</param>
    /// <returns>Validated body and bubble colors independent of cartridge reads and gameplay timing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, palette dimensions, or color values are invalid.</exception>
    public static CrystalFlashColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CrystalFlashColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CrystalFlashColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Crystal Flash color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Crystal Flash color JSON.", error);
        }
        if (document.Version != CrystalFlashColorFormat.Version)
            throw new InvalidDataException("Crystal Flash colors require the supported version.");
        return new(Compile(document.Body, CrystalFlashColorFormat.BodyFrameCount,
                CrystalFlashColorFormat.BodyColorCount, "body"),
            Compile(document.Bubble, CrystalFlashColorFormat.BubbleFrameCount,
                CrystalFlashColorFormat.BubbleColorCount, "bubble"));
    }

    /// <summary>Serializes an editable document as indented camel-case UTF-8 JSON and validates the result through <see cref="Load"/> before returning it.</summary>
    /// <param name="document">Body and bubble RGB5 frame arrays with the supported schema version.</param>
    /// <returns>Validated JSON bytes ready to store as the catalog resource.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails catalog validation.</exception>
    public static byte[] Write(CrystalFlashColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static Bgr555[][] Compile(PaletteRgb5[][]? source, int frameCount,
        int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException($"Crystal Flash {name} requires {frameCount} frames.");
        var result = new Bgr555[frameCount][];
        for (int frame = 0; frame < frameCount; frame++)
        {
            PaletteRgb5[]? colors = source[frame];
            if (colors is null || colors.Length != colorCount)
                throw new InvalidDataException(
                    $"Crystal Flash {name} frame {frame} requires {colorCount} colors.");
            result[frame] = new Bgr555[colorCount];
            for (int index = 0; index < colorCount; index++)
            {
                PaletteRgb5? rgb = colors[index];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Crystal Flash {name} frame {frame}, color {index} requires RGB5 channels 0..31.");
                result[frame][index] = rgb.ToBgr555();
            }
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Crystal Flash color property {name}."));
}

/// <summary>Editable JSON payload containing Crystal Flash body and bubble RGB5 frames; native playback timing is not part of this schema.</summary>
public sealed record CrystalFlashColorDocument
{
    /// <summary>Schema revision, required to equal <see cref="CrystalFlashColorFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Ten native body records in playback order, ten colors each.</summary>
    public required PaletteRgb5[][] Body { get; init; }
    /// <summary>Six independent bubble frames in playback order, six colors each.</summary>
    public required PaletteRgb5[][] Bubble { get; init; }
}

/// <summary>Resource identity, supported JSON version, and fixed dimensions derived from the native Crystal Flash palette programs.</summary>
public static class CrystalFlashColorFormat
{
    /// <summary>Installed presentation resource name, <c>crystal-flash-colors.json</c>.</summary>
    public const string FileName = "crystal-flash-colors.json";
    /// <summary>Supported JSON schema revision one, checked before palette compilation.</summary>
    public const int Version = 1;
    /// <summary>Ten editable body frames, one per pointer/timer record at $91:DC00-$DC27 in native playback order.</summary>
    public const int BodyFrameCount = SamusPaletteRomData.CrystalFlash.BodyRecordCount;
    /// <summary>Six editable bubble frames selected independently by the pointer table at $91:DC28-$DC33.</summary>
    public const int BubbleFrameCount = SamusPaletteRomData.CrystalFlash.BubblePaletteCount;
    /// <summary>Ten colors in each body frame, occupying sprite palette six colors 0..9 at CGRAM $E0-$E9.</summary>
    public const int BodyColorCount = SamusPaletteRomData.CrystalFlash.BodyColorCount;
    /// <summary>Six colors in each bubble frame, occupying sprite palette six colors $A..$F at CGRAM $EA-$EF.</summary>
    public const int BubbleColorCount = SamusPaletteRomData.CrystalFlash.BubbleColorCount;
}
