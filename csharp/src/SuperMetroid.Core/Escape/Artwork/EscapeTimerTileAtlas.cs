using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable four-bit OBJ characters used by the Ceres and Zebes escape timers.</summary>
public sealed class EscapeTimerTileAtlas
{
    /// <summary>Independent fill sites of the native timer typeface beyond the calculated defaults.</summary>
    internal readonly HashSet<int> fillPixels;
    /// <summary>PNG edits that change whether a calculated basis pixel uses the fill ink.</summary>
    internal readonly Dictionary<int, bool> fillOverrides;
    /// <summary>Independent PNG colors that differ from a shared source pixel or calculated outline.</summary>
    internal readonly Dictionary<int, byte> pixelOverrides;

    /// <summary>Separates selected timer contours from shared pixel relationships and calculated default fill sites.</summary>
    private EscapeTimerTileAtlas(byte[] pixels)
    {
        fillPixels = new HashSet<int>();
        fillOverrides = new Dictionary<int, bool>();
        for (int index = 0; index < pixels.Length; index++)
        {
            if (EscapeTimerGlyphDefinitions.SourcePixel(index) != index) continue;
            bool filled = pixels[index] == EscapeTimerGlyphDefinitions.FillInk(index);
            if (EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool expected))
            {
                if (filled != expected) fillOverrides.Add(index, filled);
            }
            else if (filled) fillPixels.Add(index);
        }
        pixelOverrides = Enumerable.Range(0, pixels.Length)
            .Where(index => EscapeTimerGlyphDefinitions.SourcePixel(index) != index && pixels[index] != pixels[EscapeTimerGlyphDefinitions.SourcePixel(index)])
            .ToDictionary(index => index, index => pixels[index]);
        foreach (int index in Enumerable.Range(0, pixels.Length))
            if (EscapeTimerGlyphDefinitions.SourcePixel(index) == index && !IsFill(index) &&
                pixels[index] != EscapeTimerGlyphDefinitions.Outline(index, IsFill))
                pixelOverrides.Add(index, pixels[index]);
        Artwork = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.EscapeTimerTiles), Characters());
    }

    /// <summary>The 25 timer characters as owned sprite artwork, in transfer order: character 0 is OBJ character $1E0.</summary>
    public SpriteArtwork Artwork { get; }

    /// <summary>Determines whether a pixel uses the glyph fill ink, honoring an independently edited shared pixel.</summary>
    private bool IsFill(int index)
    {
        int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
        return source != index && pixelOverrides.TryGetValue(index, out byte value)
            ? value == EscapeTimerGlyphDefinitions.FillInk(index) : BasisFill(source);
    }

    /// <summary>Resolves a basis pixel from its PNG override, native calculated fill, or retained fill-site set.</summary>
    private bool BasisFill(int index) => fillOverrides.TryGetValue(index, out bool supplied) ? supplied :
        EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool calculated) ? calculated : fillPixels.Contains(index);

    /// <summary>Resolves an edited pixel, shared source pixel, or calculated fill and outline ink.</summary>
    private byte Pixel(int index)
    {
        if (pixelOverrides.TryGetValue(index, out byte value)) return value;
        int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
        if (source != index) return Pixel(source);
        return BasisFill(index) ? EscapeTimerGlyphDefinitions.FillInk(index) : EscapeTimerGlyphDefinitions.Outline(index, IsFill);
    }

    /// <summary>Rebuilds all 25 timer characters in native four-bit planar DMA order.</summary>
    internal byte[] Characters() => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, EscapeTimerTileAtlasFormat.Width * EscapeTimerTileAtlasFormat.Height).Select(Pixel).ToArray(),
        new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height), EscapeTimerTileAtlasFormat.BitsPerPixel);

    /// <summary>Loads the indexed PNG and compiles its pixels to the cartridge's two native DMA pages.</summary>
    public static EscapeTimerTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png,
            new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        if (planar.Length != EscapeTimerTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Escape timer artwork compiled to {planar.Length} bytes; expected {EscapeTimerTileAtlasFormat.TotalByteCount}.");
        return new(image.Pixels);
    }

    /// <summary>Resolves one native page without combining or retiming its original transfer record.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        byte[] transfer = Characters();
        return asset switch
        {
            VramAssetId.EscapeTimerFirstTiles => transfer.AsMemory(0, EscapeTimerTileAtlasFormat.FirstByteCount),
            VramAssetId.EscapeTimerSecondTiles => transfer.AsMemory(EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.SecondByteCount),
            VramAssetId.None or
                VramAssetId.StandardHudTiles or
                VramAssetId.BeamPowerTiles or
                VramAssetId.BeamWaveTiles or
                VramAssetId.BeamIceTiles or
                VramAssetId.BeamIceWaveTiles or
                VramAssetId.BeamSpazerTiles or
                VramAssetId.BeamSpazerWaveTiles or
                VramAssetId.BeamSpazerIceTiles or
                VramAssetId.BeamSpazerIceWaveTiles or
                VramAssetId.BeamPlasmaTiles or
                VramAssetId.BeamPlasmaWaveTiles or
                VramAssetId.BeamPlasmaIceTiles or
                VramAssetId.BeamPlasmaIceWaveTiles or
                VramAssetId.ProjectileIceWaveTrailTiles or
                VramAssetId.ProjectileMissileTrailTiles or
                VramAssetId.GrapplePointFirstTiles or
                VramAssetId.GrapplePointSecondTiles or
                VramAssetId.GrapplePointThirdTiles or
                VramAssetId.GrapplePointFourthTiles or
                VramAssetId.GrappleHorizontalSegmentTiles or
                VramAssetId.GrappleDiagonalSegmentTiles or
                VramAssetId.GrappleVerticalSegmentTiles or
                VramAssetId.KraidBg3RestoreQuarter0 or
                VramAssetId.KraidBg3RestoreQuarter1 or
                VramAssetId.KraidBg3RestoreQuarter2 or
                VramAssetId.KraidBg3RestoreQuarter3 or
                VramAssetId.GunshipLiftoffFirstTiles or
                VramAssetId.GunshipLiftoffSecondTiles or
                VramAssetId.GunshipLiftoffThirdTiles or
                VramAssetId.GunshipLiftoffFourthTiles or
                VramAssetId.GunshipLiftoffFifthTiles or
                VramAssetId.BeamChainsawTiles or
                VramAssetId.BeamSpacetimeTiles or
                VramAssetId.StandardObjectTiles or
                VramAssetId.HudTopRowTilemap => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
            _ => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
        };
    }

}
