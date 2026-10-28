namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Installed visual compositions for extended enemy frames. Hitbox records and
/// callback pointers remain application-owned and never come from this JSON.
/// </summary>
public sealed class EnemyExtendedFrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyExtendedFrames.ContentIdentity(this);

    /// <summary>The compiled frame components and display bindings.</summary>
    internal readonly EnemyExtendedFrameSelection selection;

    /// <summary>Retains validated visual components and same-family display bindings.</summary>
    internal EnemyExtendedFrameCatalog(
        Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames) =>
        selection = new(frames, displayFrames);

    /// <summary>Finds a frame's components, including the shared empty extended frame's.</summary>
    internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        EnemyExtendedFrames.TryGet(this, frame, out components);

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        EnemyExtendedFrames.TryGetDisplay(this, frame, out components);

    /// <summary>The frame displayed for <paramref name="frame"/>, whose BG2 half accompanies the displayed components.</summary>
    internal EnemyFrameId GetDisplayFrame(EnemyFrameId frame) => EnemyExtendedFrames.GetDisplayFrame(this, frame);
}
