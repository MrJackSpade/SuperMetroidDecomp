namespace SuperMetroid.Core.Assets;

/// <summary>
/// The frame each of a room's animations shows (#1406): the display state a behaviour publishes when it selects a picture,
/// in place of copying the picture's characters into video memory. Each room builds its own, so a frame selected in one
/// room never shows in the next; the display reads the selections when it resolves the frame's pictures.
/// </summary>
public sealed class ArtworkFrameSelections
{
    /// <summary>The frame each animation shows, by animation.</summary>
    private readonly Dictionary<ArtworkAnimation, ArtworkFrame> shown = [];

    /// <summary>The animations that show a frame, with the frame each shows.</summary>
    public IReadOnlyDictionary<ArtworkAnimation, ArtworkFrame> Shown => shown;

    /// <summary>Shows <paramref name="frame"/>: from now on its animation shows it, in place of any frame selected before.</summary>
    public void Select(ArtworkFrame frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame.Animation.Name);
        ArgumentException.ThrowIfNullOrEmpty(frame.Name);
        shown[frame.Animation] = frame;
    }

    /// <summary>Stops <paramref name="animation"/> showing a frame: what was there before it first selected one shows again.</summary>
    public void Clear(ArtworkAnimation animation) => shown.Remove(animation);

    /// <summary>The frame <paramref name="animation"/> shows, if it has selected one.</summary>
    public bool TryShown(ArtworkAnimation animation, out ArtworkFrame frame) => shown.TryGetValue(animation, out frame);
}
