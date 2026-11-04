using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Whether two published frames show the same pixels (#1532). A frame's pixels are a function of its description alone: the
/// scene's records, planes, grids and shadings compare by value, as the renderer's own upload caching relies on, and every
/// other member a scene holds is immutable once published, so an equal reference holds equal content. Two frames whose
/// descriptions are equal therefore show equal pixels without drawing either; only frames whose descriptions differ are
/// drawn with the game's renderer and compared pixel by pixel. The answer is the pixel comparison's in every case.
/// </summary>
internal static class ScenePixels
{
    /// <summary>Whether <paramref name="left"/> and <paramref name="right"/> show the same pixels.</summary>
    internal static bool Same(DisplayPacket left, DisplayPacket right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return SameDescription(left, right) || DisplayRendering.Render(left).AsSpan().SequenceEqual(DisplayRendering.Render(right));
    }

    /// <summary>Whether <paramref name="left"/> and <paramref name="right"/>, each shown alone, show the same pixels.</summary>
    internal static bool Same(LayeredScene left, LayeredScene right) => Same(new DisplayPacket(0, left), new DisplayPacket(0, right));

    /// <summary>
    /// Whether two packets describe the same frame. The frame number names the publication, not what it shows, so it is not
    /// compared.
    /// </summary>
    private static bool SameDescription(DisplayPacket left, DisplayPacket right)
    {
        if (left.Brightness != right.Brightness || left.Size != right.Size || left.SolidColor != right.SolidColor)
            return false;
        if (left.Layers is { } leftScene)
            return right.Layers is { } rightScene && SameScene(leftScene, rightScene);
        if (left.Floor is { } leftFloor)
            return right.Floor is { } rightFloor && SamePaint(leftFloor.Paint, rightFloor.Paint)
                && Equals(leftFloor.Background, rightFloor.Background) && ReferenceEquals(leftFloor.Floor, rightFloor.Floor)
                && leftFloor.Brightness == rightFloor.Brightness && Equals(leftFloor.Gradient, rightFloor.Gradient);
        return right.Layers is null && right.Floor is null;
    }

    /// <summary>Whether two scenes describe the same frame: the same paint, brightness and layers in the same order.</summary>
    private static bool SameScene(LayeredScene left, LayeredScene right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left.Brightness != right.Brightness || !SamePaint(left.Paint, right.Paint))
            return false;
        ReadOnlySpan<DisplayLayer> leftLayers = left.Layers, rightLayers = right.Layers;
        if (leftLayers.Length != rightLayers.Length)
            return false;
        for (int index = 0; index < leftLayers.Length; index++)
        {
            if (!SameLayer(leftLayers[index], rightLayers[index]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Whether two layers describe the same pass. The frontend screens' layers are records whose every member compares by
    /// value: their planes, windows, gradients, tints and HUD instances. An inset scene's scene is compared by its description,
    /// a picture grid by each cell's picture, mirroring and shading, and a turning floor by its transform and its floor
    /// instance. Any other layer, such as a gameplay room's, is the same pass only when it is the same instance.
    /// </summary>
    private static bool SameLayer(DisplayLayer left, DisplayLayer right) => left switch
    {
        _ when ReferenceEquals(left, right) => true,
        InsetSceneLayer leftInset => right is InsetSceneLayer rightInset && leftInset.Left == rightInset.Left
            && leftInset.Top == rightInset.Top && leftInset.Right == rightInset.Right && leftInset.Bottom == rightInset.Bottom
            && SameScene(leftInset.Scene, rightInset.Scene),
        PictureGridLayer leftGrid => right is PictureGridLayer rightGrid && leftGrid.Scroll == rightGrid.Scroll
            && leftGrid.Priority == rightGrid.Priority && SameGrid(leftGrid.Grid, rightGrid.Grid),
        TurningFloorLayer leftFloor => right is TurningFloorLayer rightFloor && leftFloor.Transform == rightFloor.Transform
            && ReferenceEquals(leftFloor.Floor, rightFloor.Floor) && leftFloor.SubtractsSprites == rightFloor.SubtractsSprites
            && leftFloor.AddsForeground == rightFloor.AddsForeground,
        PicturedPlaneLayer or AddedPicturedPlaneLayer or SpritesLayer or SpriteDepthLayer or ScreenTintLayer or ScreenGradientLayer
            or ColourAddWindowLayer or HudLayer => left.Equals(right),
        _ => false,
    };

    /// <summary>Whether two picture grids show the same picture in every cell, mirrored and shaded alike.</summary>
    private static bool SameGrid(PictureGrid left, PictureGrid right) =>
        ReferenceEquals(left, right) || (left.Size == right.Size && left.Cells.AsSpan().SequenceEqual(right.Cells.AsSpan(), EqualityComparer<GridPicture?>.Default));

    /// <summary>Whether two paints show the same backdrop, shading and sprite draws.</summary>
    private static bool SamePaint(FramePaint left, FramePaint right) =>
        ReferenceEquals(left, right) || (left.Backdrop == right.Backdrop && Equals(left.Gameplay, right.Gameplay)
            && left.SpriteDraws.AsSpan().SequenceEqual(right.SpriteDraws.AsSpan()));
}
