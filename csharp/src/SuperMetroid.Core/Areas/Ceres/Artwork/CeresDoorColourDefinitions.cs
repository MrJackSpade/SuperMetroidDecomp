using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The background colours Ceres doors write (#1405).</summary>
internal static class CeresDoorColourDefinitions
{
    /// <summary>The six colours the door's animation cycles: the third set's colours nine to fourteen.</summary>
    internal static readonly BackgroundColourRun Animation =
        new(CeresDoorVisualRomData.AnimationTargetColor, CeresDoorVisualRomData.AnimationColorCount);
}
