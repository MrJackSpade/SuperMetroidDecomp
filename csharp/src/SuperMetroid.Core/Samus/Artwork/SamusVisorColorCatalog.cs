using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 visor colors shared by X-ray and room palette cycling.</summary>
public sealed class SamusVisorColorCatalog
{
    /// <summary>Only installed color edits differing from the reviewed $9B:A3C0 stock words.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> colors;

    /// <summary>Retains copied visor color edits independently of the mutable JSON document.</summary>
    private SamusVisorColorCatalog(Dictionary<ColourCell, Bgr555> colors) => this.colors = colors;

    /// <summary>Validates the six RGB5 visor colors and compiles independent edits relative to the reviewed $9B:A3C0 widening/cycling definitions.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled visor colors detached from the document array, without changing X-ray or room-cycle timing.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, a length other than six, null colors, or RGB5 channels outside 0..31.</exception>
    public static SamusVisorColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        SamusVisorColorDocument document;
        document = JsonAssetDocument.Read<SamusVisorColorDocument>(json, ArtworkJson.Options, "Samus visor color");
        if (document.Version != SamusVisorColorFormat.Version ||
            document.Colors is null || document.Colors.Length != SamusVisorColorFormat.ColorCount)
            throw new InvalidDataException("Samus visor colors require the supported version and six RGB5 colors.");

        var compiled = new Dictionary<ColourCell, Bgr555>();
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus visor color {index} requires RGB components from zero through 31.");
            Bgr555 packed = color.ToBgr555();
            if (packed != SamusVisorColorDefinitions.Color(index)) compiled.Add(new ColourCell(0, index), packed);
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable visor colors to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">All six RGB5 visor colors to serialize; the array is not retained.</param>
    /// <returns>Validated JSON bytes for <see cref="SamusVisorColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, color-count, or RGB5-channel validation.</exception>
    public static byte[] Write(SamusVisorColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

    /// <summary>
    /// Resolves only the six installed even offsets. Corrupted or adjacent offsets
    /// return false so the caller can enforce its bounded palette contract.
    /// </summary>
    public bool TryResolveByteOffset(int byteOffset, out Bgr555 color)
    {
        if ((byteOffset & 1) == 0 && (uint)(byteOffset >> 1) < SamusVisorColorFormat.ColorCount)
        {
            color = Resolve(byteOffset >> 1);
            return true;
        }
        color = Bgr555.Black;
        return false;
    }

    /// <summary>Returns a selected visor color for the native owners that write Samus OBJ palette-four color four, CGRAM entry 196.</summary>
    /// <param name="index">Color ordinal 0..5, not a byte offset: 0..2 are X-ray widening colors and 3..5 are the steady X-ray/room-backdrop cycle.</param>
    /// <returns>Packed SNES BGR555 word from the independent edit or calculated stock definition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
    public Bgr555 Resolve(int index)
    {
        if ((uint)index >= SamusVisorColorFormat.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors.TryGetValue(new ColourCell(0, index), out Bgr555 color) ? color : SamusVisorColorDefinitions.Color(index);
    }

}
