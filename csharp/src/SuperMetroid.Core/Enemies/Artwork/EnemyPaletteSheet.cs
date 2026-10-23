using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One editable sixteen-color RGB5 OBJ palette; selection and animation remain engine-owned.</summary>
public sealed class EnemyPaletteSheet
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("EnemyPaletteSheet-v1", content =>
        {
            content.AppendColors("colors", colors);
        });

    /// <summary>Sixteen RGB5 colors in a complete four-bit OBJ palette, including its color-zero entry.</summary>
    public const int ColorCount = 16;
    /// <summary>Owned RGB5 palette entries in OBJ color-index order.</summary>
    private readonly Bgr555[] colors;
    /// <summary>Retains the validated sixteen-color array after JSON decoding.</summary>
    private EnemyPaletteSheet(Bgr555[] colors) => this.colors = colors;

    /// <summary>Loads a version-one palette document with exactly sixteen non-null RGB5 colors, rejecting duplicate or unknown properties and channels outside 0..31; JSON property names are matched case-insensitively.</summary>
    /// <param name="json">UTF-8 JSON stream containing the selected enemy palette in color-index order.</param>
    /// <returns>An immutable sheet owning the compiled sixteen RGB5 words.</returns>
    public static EnemyPaletteSheet Load(Stream json)
    {
        EnemyPaletteSheetDocument document;
        document = JsonAssetDocument.Read<EnemyPaletteSheetDocument>(json, Options, "enemy palette");
        if (document.Version != 1 || document.Colors is null || document.Colors.Length != ColorCount)
            throw new InvalidDataException("Enemy palette requires version 1 and exactly sixteen RGB5 colors.");
        var compiled = new Bgr555[ColorCount];
        for (int i = 0; i < ColorCount; i++)
        {
            PaletteRgb5? color = document.Colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Enemy palette color {i} requires RGB channels in 0..31.");
            compiled[i] = color.ToBgr555();
        }
        return new EnemyPaletteSheet(compiled);
    }

    /// <summary>Serializes a palette document as indented camel-case UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Version-one document containing sixteen valid RGB5 colors.</param>
    /// <returns>Validated enemy-palette JSON bytes.</returns>
    public static byte[] Write(EnemyPaletteSheetDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Load(json));

    /// <summary>Copies all sixteen selected colors, including color zero, into consecutive current CGRAM entries; the room graphics-set owner selects the destination and controls palette animation.</summary>
    /// <param name="destination">The sixteen-colour OBJ palette row the room graphics-set record selects.</param>
    public void LoadTo(ColourRun destination) => destination.CopyFrom(colors);

    /// <summary>Requires case-insensitive known property names and writes indented camel-case JSON.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

}
