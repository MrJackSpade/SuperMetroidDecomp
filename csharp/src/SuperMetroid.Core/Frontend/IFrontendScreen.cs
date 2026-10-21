using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Frontend;

/// <summary>A frontend screen: the one menu or cinematic the session shows at a time.</summary>
internal interface IFrontendScreen
{
    /// <summary>Binds each installed catalog that changed from <paramref name="previous"/> to <paramref name="artwork"/>.</summary>
    /// <param name="previous">The artwork bound before.</param>
    /// <param name="artwork">The artwork to bind.</param>
    /// <param name="state">The game state running, which decides whether the screen is still on display.</param>
    void BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state);
}
