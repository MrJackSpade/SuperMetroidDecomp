namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>One consistent read of a render mailbox's private publication counters.</summary>
internal readonly record struct RenderMailboxCounterView(
    long Generation,
    long Published,
    long Taken,
    long Replaced,
    long Invalidated,
    bool HasPendingFrame);
