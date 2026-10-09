using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed RGB5 animation rows independent of cartridge storage and enemy mechanics.</summary>
public sealed class EnemyAuxiliaryColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-auxiliary-colors-v1", content =>
        {
            Span<ushort> row = stackalloc ushort[16];
            foreach (EnemyAuxiliaryPaletteDefinition definition in EnemyAuxiliaryColorDefinitions.All)
            {
                PaletteRows rows = RowsFor(definition.Id);
                content.Append("palette", (int)definition.Id);
                content.Append("frames", rows.FrameCount);
                for (int frame = 0; frame < rows.FrameCount; frame++)
                {
                    for (int color = 0; color < rows.ColorCount; color++) row[color] = rows.Color(frame, color);
                    content.AppendWords("row", row[..rows.ColorCount]);
                }
            }
        });

    /// <summary>Compiled face-block glow palette rows.</summary>
    private readonly PaletteRows faceBlock;
    /// <summary>Compiled Sidehopper drain and corpse palette rows.</summary>
    private readonly PaletteRows deadSidehopper;
    /// <summary>Compiled Golden Torizo body health-gradient rows.</summary>
    private readonly PaletteRows torizoBody;
    /// <summary>Compiled Golden Torizo rear-belly health-gradient rows.</summary>
    private readonly PaletteRows torizoBelly;

    /// <summary>Builds the lookup tables for all validated auxiliary palettes.</summary>
    /// <param name="frames">Owned RGB5 rows keyed by each required palette identity.</param>
    private EnemyAuxiliaryColorCatalog(Dictionary<EnemyAuxiliaryPalette, ushort[][]> frames)
    {
        faceBlock = new(frames[EnemyAuxiliaryPalette.FaceBlock], healthGradient: false, faceGlow: true);
        deadSidehopper = new(frames[EnemyAuxiliaryPalette.DeadSidehopper], healthGradient: false, sidehopperDrain: true);
        torizoBody = new(frames[EnemyAuxiliaryPalette.GoldenTorizoBody], healthGradient: true);
        torizoBelly = new(frames[EnemyAuxiliaryPalette.GoldenTorizoBelly], healthGradient: true, rearTorizo: true);
    }

    /// <summary>Selects the compiled row set associated with a supported auxiliary palette identity.</summary>
    /// <param name="palette">Palette family requested by the caller.</param>
    /// <returns>The compiled rows used to resolve colors for that family.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="palette"/> is not one of the four supported families.</exception>
    private PaletteRows RowsFor(EnemyAuxiliaryPalette palette) => palette switch
    {
        EnemyAuxiliaryPalette.FaceBlock => faceBlock,
        EnemyAuxiliaryPalette.DeadSidehopper => deadSidehopper,
        EnemyAuxiliaryPalette.GoldenTorizoBody => torizoBody,
        EnemyAuxiliaryPalette.GoldenTorizoBelly => torizoBelly,
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };

    /// <summary>Resolves one selected RGB5 word from the face-block glow, sidehopper drain/corpse, or Golden Torizo health rows without advancing timers, choosing health bands, or writing CGRAM.</summary>
    /// <param name="palette">One of the four installed auxiliary-palette identities.</param>
    /// <param name="frame">Zero-based row: 0..7 for face-block or Torizo palettes, or 0..6 for sidehopper stages; rows are not necessarily elapsed animation frames.</param>
    /// <param name="color">Zero-based supplied color: 0..3 for face-block, 0..14 for sidehopper, or 0..15 for Torizo; sidehopper entries omit the native row's color zero.</param>
    /// <returns>The packed RGB5 word preserving independently supplied edits.</returns>
    public ushort Resolve(EnemyAuxiliaryPalette palette, int frame, int color)
    {
        PaletteRows rows = RowsFor(palette);
        if ((uint)frame >= rows.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= rows.ColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        return rows.Color(frame, color);
    }

    /// <summary>
    /// $84:8032/8132 Golden Torizo body/belly health palettes interpolate RGB5 endpoints
    /// to nearest over seven intervals. Transparent slot zero holds its initial word until
    /// the last band. $A8:E7CC face-block glow mirrors a three-step rise; its fourth color
    /// has a separate resting color and two-step active rise. Endpoint paint and unmatched
    /// independently supplied rows remain data. $A9:EBCC-EC7C Sidehopper draining interpolates
    /// actual RGB5 endpoints to nearest over five intervals; EC8C is separate corpse paint.
    /// </summary>
    private sealed class PaletteRows
    {
        /// <summary>First supplied row, used as the starting endpoint for calculated transitions.</summary>
        private readonly ushort[] first;
        /// <summary>Final endpoint row for health gradients, glow cycles, and drain stages.</summary>
        private readonly ushort[] last;
        /// <summary>Complete supplied rows retained when the calculated relationship would change an installed color.</summary>
        private readonly ushort[][]? supplied;
        /// <summary>Selects the face-block's three-step glow cycle and special fourth-color accent handling.</summary>
        private readonly bool faceGlow;
        /// <summary>Mode flags for canonical paint fast paths: Torizo and face glow select their matching stock catalogs; sidehopper selects its drain catalog, while rearTorizo chooses the belly variant.</summary>
        private readonly bool stockTorizo, rearTorizo, stockSidehopperDrain, stockFaceGlow;
        /// <summary>Active face-block fourth-color endpoint, distinct from its resting color.</summary>
        private readonly ushort glowAccentStart;
        /// <summary>Separate Sidehopper corpse-palette row, which is not part of the drain interpolation.</summary>
        private readonly ushort[]? corpse;

        /// <summary>Number of supplied animation or health-band rows.</summary>
        internal int FrameCount { get; }
        /// <summary>Number of packed RGB5 colors in each supplied row.</summary>
        internal int ColorCount { get; }

        /// <summary>Compiles palette-specific transitions and keeps original rows whenever the native relationship is not exact.</summary>
        /// <param name="rows">Validated packed RGB5 rows for one palette family.</param>
        /// <param name="healthGradient">Enables Golden Torizo's per-health-band endpoint interpolation.</param>
        /// <param name="faceGlow">Enables the face-block's mirrored glow cycle and accent endpoint.</param>
        /// <param name="sidehopperDrain">Enables the Sidehopper drain progression with its separate corpse row.</param>
        /// <param name="rearTorizo">Selects the belly variant when comparing against the stock Torizo palette.</param>
        internal PaletteRows(ushort[][] rows, bool healthGradient, bool faceGlow = false, bool sidehopperDrain = false, bool rearTorizo = false)
        {
            FrameCount = rows.Length;
            ColorCount = rows[0].Length;
            if (healthGradient && GoldenTorizoHealthPaintDefinitions.Matches(rows, rearTorizo))
            {
                stockTorizo = true; this.rearTorizo = rearTorizo; first = []; last = []; return;
            }
            if (faceGlow && FaceBlockGlowPaintDefinitions.Matches(rows))
            {
                stockFaceGlow = true; first = []; last = []; return;
            }
            if (sidehopperDrain && SidehopperDrainPaintDefinitions.Matches(rows))
            {
                stockSidehopperDrain = true; first = []; last = []; return;
            }
            first = rows[0];
            last = rows[faceGlow ? 3 : sidehopperDrain ? rows.Length - 2 : rows.Length - 1];
            corpse = sidehopperDrain ? rows[^1] : null;
            this.faceGlow = faceGlow;
            glowAccentStart = faceGlow ? rows[1][3] : (ushort)0;
            if (!healthGradient && !faceGlow && !sidehopperDrain) { supplied = rows; return; }
            for (int frame = 0; frame < FrameCount; frame++)
                for (int color = 0; color < ColorCount; color++)
                    if (Calculate(frame, color) != rows[frame][color]) { supplied = rows; return; }

        }
        /// <summary>Returns an exact installed entry when needed, otherwise the corresponding calculated transition color.</summary>
        /// <param name="frame">Row index in the palette's authored stage sequence.</param>
        /// <param name="color">Color index in that row.</param>
        internal ushort Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Calculates a palette entry from stock data or the endpoints and progression mode selected at construction.</summary>
        /// <param name="frame">Row index in the authored sequence.</param>
        /// <param name="color">Color index within the palette row.</param>
        /// <returns>The packed RGB5 value for that stage and color.</returns>
        private ushort Calculate(int frame, int color)
        {
            if (stockTorizo) return GoldenTorizoHealthPaintDefinitions.Color(frame, color, rearTorizo);
            if (stockFaceGlow) return FaceBlockGlowPaintDefinitions.Color(frame, color);
            if (stockSidehopperDrain) return SidehopperDrainPaintDefinitions.Color(frame, color);
            if (corpse is not null && frame == FrameCount - 1) return corpse[color];
            int steps = faceGlow ? 3 : FrameCount - (corpse is null ? 1 : 2);
            if (faceGlow) frame = Math.Min(frame, FrameCount - 1 - frame);
            else if (corpse is null && color == 0) return frame == steps ? last[color] : first[color];
            ushort start = first[color];
            if (faceGlow && color == 3)
            {
                if (frame == 0) return start;
                start = glowAccentStart;
                frame--; steps--;
            }
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((start >> shift & 31) * (steps - frame)
                    + (last[color] >> shift & 31) * frame + steps / 2) / steps) << shift;
            return (ushort)result;
        }
    }
    /// <summary>Loads all four named palette families at the supported version, validating each definition's row/color counts and RGB5 channels 0..31 while rejecting duplicate or unknown properties; compiles owned rows independently of cartridge storage.</summary>
    /// <param name="source">UTF-8 JSON stream containing enum-named palette families; integer enum identities are not accepted.</param>
    /// <returns>The immutable auxiliary-color catalog, retaining supplied rows when they differ from calculated stock relationships.</returns>
    public static EnemyAuxiliaryColorCatalog Load(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        EnemyAuxiliaryColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(source);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyAuxiliaryColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Enemy auxiliary colors are null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid enemy auxiliary color JSON.", error); }
        if (document.Version != EnemyAuxiliaryColorFormat.Version || document.Palettes is null ||
            document.Palettes.Count != EnemyAuxiliaryColorDefinitions.All.Length)
            throw new InvalidDataException("Enemy auxiliary colors require version one and all four named palettes.");
        var result = new Dictionary<EnemyAuxiliaryPalette, ushort[][]>();
        foreach (EnemyAuxiliaryPaletteDefinition definition in EnemyAuxiliaryColorDefinitions.All)
        {
            if (!document.Palettes.TryGetValue(definition.Id, out PaletteRgb5[][]? rows) ||
                rows is null || rows.Length != definition.FrameCount)
                throw new InvalidDataException($"Palette {definition.Id} requires {definition.FrameCount} frames.");
            var compiled = new ushort[rows.Length][];
            for (int frame = 0; frame < rows.Length; frame++)
            {
                PaletteRgb5[]? row = rows[frame];
                if (row is null || row.Length != definition.ColorCount)
                    throw new InvalidDataException($"Palette {definition.Id} frame {frame} requires {definition.ColorCount} colors.");
                compiled[frame] = new ushort[row.Length];
                for (int color = 0; color < row.Length; color++)
                {
                    PaletteRgb5? rgb = row[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Palette {definition.Id} frame {frame} color {color} requires RGB5 channels 0..31.");
                    compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                }
            }
            result.Add(definition.Id, compiled);
        }
        return new EnemyAuxiliaryColorCatalog(result);
    }

    /// <summary>Serializes the auxiliary-color document as indented camel-case UTF-8 JSON with named palette identities and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and every palette family's complete RGB5 rows.</param>
    /// <returns>Validated auxiliary-color JSON bytes.</returns>
    public static byte[] Write(EnemyAuxiliaryColorDocument document)
    {
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(data, writable: false));
        return data;
    }

    /// <summary>Rejects repeated object properties before deserialization can collapse duplicate palette data.</summary>
    /// <param name="value">Parsed JSON value whose object properties are checked recursively.</param>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.OrdinalIgnoreCase,
            name => new InvalidDataException($"Duplicate auxiliary palette property {name}."));

    /// <summary>JSON settings requiring camel-case names, named palette enums, and rejection of unknown properties.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter<EnemyAuxiliaryPalette>(allowIntegerValues: false) },
    };
}

/// <summary>Editable JSON schema for four auxiliary enemy palette families; runtime AI retains palette destination, update cadence, drain-stage progression, and health-band selection.</summary>
public sealed record EnemyAuxiliaryColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="EnemyAuxiliaryColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All four enum-named RGB5 families: FaceBlock has eight four-color rows, DeadSidehopper seven fifteen-color rows, and each Golden Torizo family eight sixteen-color health bands.</summary>
    public required Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]> Palettes { get; init; }
}
