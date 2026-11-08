namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The state of Crocomire's tongue actor.</summary>
public sealed class CrocomireTongueState
{
    /// <summary>The tongue's horizontal offset from the body, in pixels, added to the body's X every update.</summary>
    public int BodyXOffset { get; internal set; }
}
