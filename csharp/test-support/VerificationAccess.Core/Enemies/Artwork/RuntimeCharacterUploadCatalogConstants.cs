using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="RuntimeCharacterUploadCatalog"/> that only verification reads.</summary>
internal static class RuntimeCharacterUploadCatalogConstants
{
    /// <summary>
    /// The regions <paramref name="uploads"/> divide the characters into: each run of bytes between their bounds that
    /// one of them covers, which each covers whole or not at all.
    /// </summary>
    public static IReadOnlyList<CharacterRegion> Regions(IReadOnlyList<RuntimeCharacterUpload> uploads)
    {
        int[] bounds = [.. Ensure.NotNull(uploads).SelectMany(upload => new[] { upload.FirstByte, upload.FirstByte + upload.ByteCount }).Distinct().Order()];
        var regions = new List<CharacterRegion>();
        for (int index = 0; index + 1 < bounds.Length; index++)
        {
            var region = new CharacterRegion(bounds[index], bounds[index + 1] - bounds[index]);
            if (RuntimeCharacterUploadCatalog.Over(uploads, region).Any())
                regions.Add(region);
        }
        return regions;
    }
}
