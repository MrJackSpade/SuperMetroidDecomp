using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The charge/Hyper flare and its directional sparks: each composition is named by the picture it shows (#1762).</summary>
public static class ChargeFlareSpriteDefinitions
{
    /// <summary>The main flare's pictures, in the order the flare shows them; both facings show the same.</summary>
    internal static readonly ChargeFlarePicture[] MainPictures =
    [
        ChargeFlarePicture.MainFirst, ChargeFlarePicture.MainSecond, ChargeFlarePicture.MainThird, ChargeFlarePicture.MainFourth,
    ];

    /// <summary>A spark ring's pictures, in the order the ring shows them; each facing's rings show their own.</summary>
    internal static readonly ChargeFlarePicture[] SparkPictures =
    [
        ChargeFlarePicture.SparksFirst, ChargeFlarePicture.SparksSecond, ChargeFlarePicture.SparksThird,
        ChargeFlarePicture.SparksFourth, ChargeFlarePicture.SparksFifth, ChargeFlarePicture.SparksSixth,
    ];

    /// <summary>The spark rings' families: the right slow, right fast, left slow and left fast sparks.</summary>
    internal static readonly ProjectileSpriteGroup[] SparkFamilies =
    [
        ProjectileSpriteGroup.FlareSlowSparksRight, ProjectileSpriteGroup.FlareFastSparksRight,
        ProjectileSpriteGroup.FlareSlowSparksLeft, ProjectileSpriteGroup.FlareFastSparksLeft,
    ];

    /// <summary>The twenty-eight distinct flare and spark compositions: the main flare's, then each spark family's.</summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } =
    [
        .. MainPictures.Select(picture => new ProjectileSpriteId(ProjectileSpriteGroup.FlareMain, picture.ToString())),
        .. SparkFamilies.SelectMany(family => SparkPictures.Select(picture => new ProjectileSpriteId(family, picture.ToString()))),
    ];
}
