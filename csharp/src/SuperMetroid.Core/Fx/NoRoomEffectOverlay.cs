namespace SuperMetroid.Core.Fx;

/// <summary>The effect overlay of a room whose effect shows nothing on it (#2505).</summary>
public sealed record NoRoomEffectOverlay : RoomEffectOverlay
{
    /// <summary>The one overlay showing nothing, <see cref="RoomEffectOverlay.Nothing"/>.</summary>
    internal NoRoomEffectOverlay()
    {
    }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IRoomEffectOverlayMapping<TResult> mapping) => Ensure.NotNull(mapping).Nothing();
}
