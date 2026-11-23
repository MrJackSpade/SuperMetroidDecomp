using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A frontend screen the display publishes by composing its scene (#2557): the title, the file-select, options, pause and
/// game-over menus, the file-select map and the Ceres destruction. Composing reads the screen as it stands; it never
/// advances it.
/// </summary>
/// <typeparam name="TScene">The kind of scene the screen composes.</typeparam>
internal interface IComposedFrontendScene<out TScene>
    where TScene : DisplayScene
{
    /// <summary>Composes the screen's scene as it stands.</summary>
    TScene ComposeScene();
}
