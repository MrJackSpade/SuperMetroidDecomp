namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>One consistent read of a render mailbox's generation, replacements and pending frame.</summary>
internal readonly record struct RenderMailboxCounterView(
    long Generation,
    long Replaced,
    bool HasPendingFrame);
