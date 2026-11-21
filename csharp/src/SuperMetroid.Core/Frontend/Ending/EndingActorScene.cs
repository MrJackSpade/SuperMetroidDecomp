using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A scene of the ending with actors of its own to draw (#2553): every scene but the final logo's item-percentage drop and
/// its typed texts, which show their screen alone.
/// </summary>
internal abstract class EndingActorScene : EndingScene
{
    /// <summary>Only the ending's scenes with actors derive from it.</summary>
    private protected EndingActorScene()
    {
    }

    /// <summary>Places the scene's actors among the frame's sprites: the scene's own, or the cast each on its role's layer.</summary>
    /// <param name="ending">The ending showing the scene.</param>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="shows">Whether the sprites of a layer are placed.</param>
    internal abstract void DrawActors(EndingCreditsState ending, ScreenSprites sprites, Func<EndingSpriteLayer, bool> shows);
}
