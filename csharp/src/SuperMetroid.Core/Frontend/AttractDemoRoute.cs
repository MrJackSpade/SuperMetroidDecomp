using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>Where the attract mode goes after a demonstration scene (#2560, #2553).</summary>
internal abstract record AttractDemoRoute
{
    /// <summary>Only the routes below exist.</summary>
    private AttractDemoRoute()
    {
    }

    /// <summary>The set's next scene loads.</summary>
    /// <param name="Scene">The scene the set plays next.</param>
    internal sealed record NextScene(AttractDemoScene Scene) : AttractDemoRoute;

    /// <summary>The title returns at its title screen, as after a cancelled scene.</summary>
    internal sealed record TitleAtItsScreen : AttractDemoRoute;

    /// <summary>The title starts from its beginning, once the set has played through.</summary>
    internal sealed record TitleFromBeginning : AttractDemoRoute;
}
