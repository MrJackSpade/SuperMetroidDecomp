using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// A draw of one of Samus's pictures (#2505): her body, her equipment's or her weapons' frame placed unmirrored at an
/// origin, shown in her colours as they stand when the frame publishes. Her draws are made before her colours settle for
/// the frame, so the frame's publication (<see cref="SamusColours.ShadeOwnDraws"/>) makes each a draw naming her shading.
/// </summary>
public sealed record SamusColouredDraw : SpriteDraw
{
    /// <summary>Places one of Samus's sheet frames.</summary>
    /// <param name="source">Which of Samus's sprites is shown.</param>
    /// <param name="image">The sheet frame shown.</param>
    /// <param name="origin">Where on the screen the frame's origin lands; left of or above the screen is negative.</param>
    public SamusColouredDraw(SpriteSource source, SpriteSheetFrame image, Offset16<ScreenPixels> origin)
        : base(source, origin) => Image = Ensure.NotNull(image);

    /// <summary>The sheet frame shown.</summary>
    private SpriteSheetFrame Image { get; }

    /// <inheritdoc/>
    internal override ImageSpriteDraw Published(PictureShading samusShading) =>
        new(Source, Image, Origin, SpriteMirror.None) { Shading = samusShading };

    /// <summary>Names the draw by its source, image and screen origin.</summary>
    public override string ToString() =>
        $"{Source.Kind} {Image.Sheet.Name} at ({Origin.X}, {Origin.Y})";
}
