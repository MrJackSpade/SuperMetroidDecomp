using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The shades the import pre-colours pictures in for the colour effects' frames (#1512, #1598): one per colour group and
/// frame the group shows, which colours the pictures of that group show while the frame is the group's last. Each of an
/// animation's frames in one colour group is a step of one shade effect, a colour animation named for the group and the
/// animation (#1712).
/// </summary>
public static class ColourEffectShades
{
    /// <summary>The shade of <paramref name="colours"/> showing <paramref name="frame"/>.</summary>
    public static PictureShade Of(ColourEffectColours colours, ColourEffectFrame frame) =>
        new($"colour-animation-{colours}-{frame.Animation}", frame.Frame);
}
