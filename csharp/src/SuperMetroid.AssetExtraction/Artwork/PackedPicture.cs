using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One picture packed onto a sheet (#1392, #1513): the picture and its rectangle's top-left corner.</summary>
/// <typeparam name="TPicture">The picture packed.</typeparam>
/// <param name="Picture">The picture.</param>
/// <param name="Corner">Its rectangle's top-left corner on the sheet; the sheet's corner for an empty picture.</param>
internal sealed partial record PackedPicture<TPicture>(TPicture Picture, Position16<ImagePixels> Corner);
