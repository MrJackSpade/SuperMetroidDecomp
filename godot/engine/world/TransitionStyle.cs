namespace Engine.World;

/// <summary>How the change from one room to the next looks.</summary>
public enum TransitionStyle
{
    /// <summary>The view scrolls sideways into the next room.</summary>
    ScrollAcross,

    /// <summary>The view scrolls up or down into the next room.</summary>
    ScrollVertically,

    /// <summary>The view fades out and back in.</summary>
    Fade,
}
