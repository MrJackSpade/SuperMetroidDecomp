namespace SuperMetroid.Core.Assets;

/// <summary>
/// What a gameplay blend changes (#1514): the pixels where the room's foreground, its background, an actor taking part in
/// blends or the backdrop is the frontmost thing shown. Pixels where something else is frontmost keep their colour.
/// </summary>
/// <remarks>The members combine freely; their values are arbitrary.</remarks>
[Flags]
public enum BlendedLayers
{
    /// <summary>The room's foreground blends where it is frontmost.</summary>
    Foreground = 1 << 0,
    /// <summary>The room's background blends where it is frontmost.</summary>
    Background = 1 << 1,
    /// <summary>
    /// The actors whose colours take part in blends blend where they are frontmost; an actor drawn in colours that never
    /// blend keeps its colour.
    /// </summary>
    Actors = 1 << 2,
    /// <summary>The backdrop blends where nothing covers it.</summary>
    Backdrop = 1 << 3,
}
