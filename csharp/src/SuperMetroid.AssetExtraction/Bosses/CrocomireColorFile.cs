using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Crocomire colour document.</summary>
public static class CrocomireColorFile
{
    /// <summary>Loads version-1 <c>crocomire-colors.json</c>, validating the five native transfer lengths and RGB5 channel bounds while keeping hurt-flash and death transitions in actor code.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Selected compiled inks, preserving independent supplied edits even where stock source transfers overlap.</returns>
    public static CrocomireColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        CrocomireColorDocument document;
        document = JsonAssetDocument.Read<CrocomireColorDocument>(json, ArtworkJson.Options, "Crocomire color");
        if (document.Version != CrocomireColorFormat.Version)
            throw new InvalidDataException("Crocomire colors require the supported version.");
        return new(Edits(new()
        {
            [CrocomireColorCatalog.Band.FightBody] =
                Compile(document.FightBody, CrocomireColorDefinitions.FightBodyCount, "fight body"),
            [CrocomireColorCatalog.Band.InitialWall] =
                Compile(document.InitialWall, CrocomireColorDefinitions.InitialWallCount, "initial wall"),
            [CrocomireColorCatalog.Band.InitialProjectile] =
                Compile(document.InitialProjectile, CrocomireColorDefinitions.InitialProjectileCount, "initial projectile"),
            [CrocomireColorCatalog.Band.SkeletonArm] =
                Compile(document.SkeletonArm, CrocomireColorDefinitions.SkeletonArmCount, "skeleton arm"),
            [CrocomireColorCatalog.Band.WallSpikes] =
                Compile(document.WallSpikes, CrocomireColorDefinitions.WallSpikesCount, "wall spikes"),
        }));
    }

    /// <summary>Keeps each supplied color of every band that differs from its calculated stock paint.</summary>
    private static Dictionary<CrocomireColorKey, RgbColor> Edits(Dictionary<CrocomireColorCatalog.Band, RgbColor[]> supplied)
    {
        var edits = new Dictionary<CrocomireColorKey, RgbColor>();
        foreach ((CrocomireColorCatalog.Band band, RgbColor[] colors) in supplied)
        {
            for (int color = 0; color < colors.Length; color++)
                if (colors[color] != CrocomireColorCatalog.StockPaint[band](color))
                    edits.Add(new CrocomireColorKey(band, color), colors[color]);
        }
        return edits;
    }

    /// <summary>Serializes the five editable color bands as indented camel-case UTF-8 JSON, validating version, transfer lengths, and RGB5 channels before returning the bytes.</summary>
    public static byte[] Write(CrocomireColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Validates a band's length and RGB5 range before packing its color words.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Crocomire {name}");
}
