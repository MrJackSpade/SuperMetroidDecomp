using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>An ending actor that shows pictures of its role's sprite family (#2197, #2489).</summary>
/// <param name="picture">The actor.</param>
/// <param name="role">What the actor shows and how it moves.</param>
internal sealed class EndingPictureSprite(CinematicActor<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue> picture, EndingSpriteRole role)
    : EndingSprite(role)
{
    /// <summary>The actor, showing its family's pictures.</summary>
    internal CinematicActor<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue> Picture { get; } = Ensure.NotNull(picture);

    /// <inheritdoc/>
    public override ICinematicActor<EndingSceneCue> Sprite => Picture;

    /// <inheritdoc/>
    internal override void Draw(ScreenSprites screenSprites, EndingSpriteArt art) =>
        Picture.Draw(screenSprites, installedArt: Ensure.NotNull(art).Pictures(Definition.Pictures));
}
