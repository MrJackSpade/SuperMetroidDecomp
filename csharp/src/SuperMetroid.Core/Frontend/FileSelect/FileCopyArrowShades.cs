using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The shades the import pre-colours the file-copy arrows in (#1512): each step of the rotation that moves the arrows'
/// colours one place along. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class FileCopyArrowShades
{
    /// <summary>The arrows after <paramref name="step"/> rotations, below <see cref="FileCopyArrowDefinitions.RotationSteps"/>.</summary>
    public static PictureShade Rotation(int step) => new("FileCopyArrowRotation", step);
}
