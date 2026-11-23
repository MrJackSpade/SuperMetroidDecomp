namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed key of each <see cref="WrittenPictureLayer"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class WrittenPictureLayerKeys
{
    /// <summary>Each WrittenPictureLayer member's installed key.</summary>
    internal static InstalledKeys<WrittenPictureLayer> Installed { get; } = new(new Dictionary<WrittenPictureLayer, string>
    {
        [WrittenPictureLayer.WrittenOverRoom] = "WrittenOverRoom",
        [WrittenPictureLayer.FxOverlay] = "FxOverlay",
    });
}
