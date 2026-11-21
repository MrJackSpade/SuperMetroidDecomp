using SuperMetroid.AssetExtraction.Frontend;
namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One picture of a screen sheet by its frame name, which the game knows without the import's artwork, and how the import
/// lays the picture out from <typeparamref name="TSource"/>, so one list yields both the names and the pictures.
/// </summary>
/// <typeparam name="TSource">What the picture is laid out from.</typeparam>
/// <param name="Name">The picture's frame name.</param>
/// <param name="Picture">Lays the picture, named <paramref name="Name"/>, out from its source.</param>
internal sealed record NamedScreenPicture<TSource>(string Name, Func<TSource, ScreenPictureSubject> Picture);
