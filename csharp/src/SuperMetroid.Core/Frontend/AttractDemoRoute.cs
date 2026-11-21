namespace SuperMetroid.Core.Frontend;

/// <summary>Where the attract mode goes after a demonstration scene (#2560).</summary>
internal enum AttractDemoRoute
{
    /// <summary>The set's next scene loads.</summary>
    NextScene,
    /// <summary>The title returns at its title screen, as after a cancelled scene.</summary>
    TitleAtItsScreen,
    /// <summary>The title starts from its beginning, once the set has played through.</summary>
    TitleFromBeginning,
}
