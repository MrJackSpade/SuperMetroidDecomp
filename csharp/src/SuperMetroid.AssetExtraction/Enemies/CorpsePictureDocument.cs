using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where a rotting corpse lies in a picture, as the enemy sheet manifest spells it (#1539): the picture row of the
/// corpse's first pixel row, from which the corpse's rows rot across the whole picture.
/// </summary>
internal sealed record CorpsePictureDocument
{
    /// <summary>The picture row the corpse's first pixel row lies at.</summary>
    public required int Top { get; init; }

    /// <summary><paramref name="placement"/>, as the manifest spells it.</summary>
    internal static CorpsePictureDocument Of(CorpsePicturePlacement placement) => new() { Top = Ensure.NotNull(placement).Top };

    /// <summary>The placement the manifest spells.</summary>
    internal CorpsePicturePlacement Placement() => new(Top);
}
