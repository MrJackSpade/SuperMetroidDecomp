using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A sheet's pictures as <see cref="SheetPacking"/> packs them: the sheet's size and each picture where it lies.</summary>
/// <typeparam name="TPicture">The pictures packed.</typeparam>
/// <param name="Size">The sheet's width and height.</param>
/// <param name="Pictures">Each picture with its rectangle's top-left corner on the sheet, in the order they were packed.</param>
internal sealed partial record PackedSheet<TPicture>(Extent16<ImagePixels> Size, IReadOnlyList<PackedPicture<TPicture>> Pictures);
