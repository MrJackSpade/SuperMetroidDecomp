using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The shades the import pre-colours the file-copy arrows in (#1512): each step of the rotation that moves the arrows'
/// colours one place along. Which inks each recolours, and to what, is the import's.
/// </summary>
public static partial class FileCopyArrowShades
{
    /// <summary>The arrows at rotation step <paramref name="step"/>.</summary>
    public static PictureShade Rotation(FileCopyArrowRotationStep step) => new("FileCopyArrowRotation", step.Step);
}
