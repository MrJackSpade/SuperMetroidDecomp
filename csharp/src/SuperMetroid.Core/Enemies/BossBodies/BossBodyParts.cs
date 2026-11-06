namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body as its parts show it (#1515): the frame each part of the boss last showed on it, a later part's frame over
/// the frames of the parts before it in the room's enemy list, and the row from which the body is hidden. Phantoon's body,
/// eye, tentacles and mouth, Draygon's body and eyes, Crocomire's body and melt images, and Mother Brain's body show their
/// frames here; each frame shown is the part's current one, whatever it covered before.
/// </summary>
internal sealed class BossBodyParts
{
    /// <summary>The frame each part shows, by its place in the room's enemy list.</summary>
    private readonly SortedDictionary<int, BossBodyFrame> frames = [];

    /// <summary>The body row from which nothing shows, or null when the whole body shows.</summary>
    private int? hiddenFrom;

    /// <summary>The body as it stands, until a part shows another frame or the body is cleared or hidden.</summary>
    private BossBodyLayer? published;

    /// <summary>A body no part shows a frame on yet, shown from the start unless <paramref name="shown"/> is false.</summary>
    /// <param name="shown">Whether the room shows the body from the start; Mother Brain's shows only once her second phase begins.</param>
    public BossBodyParts(bool shown = true) => IsShown = shown;

    /// <summary>Whether the room shows the body: false only while Mother Brain readies hers before her second phase (#1356).</summary>
    public bool IsShown { get; private set; }

    /// <summary>The room shows the body from now on, as Mother Brain's second phase begins (#1356).</summary>
    public void Reveal() => IsShown = true;

    /// <summary>
    /// The part at <paramref name="part"/> in the room's enemy list shows <paramref name="frame"/> from now on. A frame shown
    /// shows the body whole again.
    /// </summary>
    public void Show(int part, BossBodyFrame frame)
    {
        frames[part] = Ensure.NotNull(frame);
        hiddenFrom = null;
        published = null;
    }

    /// <summary>No part shows anything on the body, as a boss's death clears it.</summary>
    public void Clear()
    {
        frames.Clear();
        hiddenFrom = null;
        published = null;
    }

    /// <summary>
    /// Hides the body from body row <paramref name="top"/> down until a part next shows a frame; a lower row hides nothing
    /// more than is hidden already.
    /// </summary>
    public void HideFrom(int top)
    {
        if (hiddenFrom is { } hidden && hidden <= top)
            return;
        hiddenFrom = top;
        published = null;
    }

    /// <summary>The body as it stands, shared until it changes.</summary>
    public BossBodyLayer Publish() => published ??= new BossBodyLayer([.. frames.Values], hiddenFrom);
}
