using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>An ending actor that shows pictures of its role's sprite family (#2197, #2489).</summary>
internal sealed partial class EndingPictureSprite
    : EndingSprite
{
    /// <summary>Creates an <see cref="EndingPictureSprite"/>.</summary>
    /// <param name="picture">The actor.</param>
    /// <param name="role">What the actor shows and how it moves.</param>
    public EndingPictureSprite(CinematicActor<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue> picture, EndingSpriteRole role)
        : base(role)
    {
        this.Picture = Ensure.NotNull(picture);
    }

    /// <summary>The actor, showing its family's pictures.</summary>
    internal CinematicActor<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue> Picture { get; }

    /// <inheritdoc/>
    public override ICinematicActor<EndingSceneCue> Sprite => Picture;

    /// <inheritdoc/>
    internal override void Draw(ScreenSprites screenSprites, EndingSpriteArt art) =>
        Picture.Draw(screenSprites, installedArt: Ensure.NotNull(art).Pictures(Definition.Pictures));
}
