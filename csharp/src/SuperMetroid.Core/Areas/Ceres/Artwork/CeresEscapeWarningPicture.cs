namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One picture of the Ceres escape warning (#1466, #1555): the EMERGENCY title or a row half of the Japanese subtitle.</summary>
/// <param name="Name">The picture's installed name, which names the lettering it shows.</param>
/// <param name="Corner">The pixel of the room's foreground, from its top-left, its top-left corner shows at (#1406, #1514, #1601).</param>
internal readonly record struct CeresEscapeWarningPicture(string Name, Position16<PlanePixels> Corner)
{
    /// <summary>
    /// The name of the picture in the room's written picture sheet (#1466): the getaway writes the picture by it, and
    /// extraction installs the picture of its lettering under it.
    /// </summary>
    internal string Picture => $"CeresWarning.{Name}";
}
