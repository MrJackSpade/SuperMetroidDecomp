namespace Engine.World;

/// <summary>How one screen of a room lets the view in.</summary>
public enum ScreenScroll
{
    /// <summary>The view is kept out of it.</summary>
    Closed,

    /// <summary>The view may show it, and may rest with its bottom at the screen's bottom.</summary>
    Open,

    /// <summary>The view may show it, and rests with its top at the screen's top.</summary>
    OpenAlignedToTop,
}
