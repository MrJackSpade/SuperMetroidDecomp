using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The shades the import pre-colours Phantoon's body sheet and his room's pictures in (#1512, #2251): each settled
/// colouring of his body, and the room once the Wrecked Ship's power has come on. His fades and the power coming on are
/// typed fades between them (<see cref="PictureShadeFade"/>).
/// </summary>
public static partial class PhantoonShades
{
    /// <summary>His body showing <paramref name="look"/>.</summary>
    public static PictureShade Body(PhantoonBodyLook look) => new($"phantoon-body-{look.Key}", 0);

    /// <summary>The room once the Wrecked Ship's power has come on.</summary>
    public static PictureShade Powered => new("phantoon-powered", 0);
}
