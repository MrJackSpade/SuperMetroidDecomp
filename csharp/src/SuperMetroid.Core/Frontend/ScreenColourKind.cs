namespace SuperMetroid.Core.Frontend;

/// <summary>Which half of a frontend screen's colours a colour group lies in: the background layers' or the sprites'.</summary>
internal enum ScreenColourKind
{
    /// <summary>The colours the screen's background layers show.</summary>
    Background,

    /// <summary>The colours the screen's sprites show.</summary>
    Sprite,
}
