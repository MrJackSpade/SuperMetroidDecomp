using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>An ending actor that types one of the ending's texts (#2195): each frame is the text typed so far.</summary>
internal sealed partial class EndingTextSprite
    : EndingSprite
{
    /// <summary>Creates an <see cref="EndingTextSprite"/>.</summary>
    /// <param name="text">The actor.</param>
    /// <param name="role">What the actor shows.</param>
    public EndingTextSprite(CinematicActor<EndingTypedText, EndingTextTyping, EndingSceneCue> text, EndingSpriteRole role)
        : base(role)
    {
        this.Text = Ensure.NotNull(text);
    }

    /// <summary>The actor, showing its text typed so far.</summary>
    public CinematicActor<EndingTypedText, EndingTextTyping, EndingSceneCue> Text { get; }

    /// <inheritdoc/>
    public override ICinematicActor<EndingSceneCue> Sprite => Text;

    /// <inheritdoc/>
    internal override void Draw(ScreenSprites screenSprites, EndingSpriteArt art)
    {
        Ensure.NotNull(art);
        if (Text.Drawing() is { } drawing)
            EndingTypedTextDrawing.DrawText(art.TextSheet, drawing.Shown, screenSprites, drawing.Place, art.Shading(drawing.Colouring));
    }
}
