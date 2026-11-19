namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Reads and writes a <see cref="WrittenPictureLayer"/> in an installed document by its installed key (#2520).</summary>
internal sealed class WrittenPictureLayerJsonConverter() : InstalledKeyJsonConverter<WrittenPictureLayer>(WrittenPictureLayerKeys.Installed);
