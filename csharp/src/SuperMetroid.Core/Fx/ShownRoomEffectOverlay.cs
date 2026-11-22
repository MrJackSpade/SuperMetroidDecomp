namespace SuperMetroid.Core.Fx;

/// <summary>
/// A room effect's liquid or atmosphere shown on the effect overlay (#1706, #2505), with how it blends: lying over the
/// room or behind it.
/// </summary>
public abstract partial record ShownRoomEffectOverlay : RoomEffectOverlay
{
    /// <summary>Keeps how the overlay blends; only the liquid and the atmosphere derive from this.</summary>
    /// <param name="blend">How the overlay blends with the room.</param>
    private protected ShownRoomEffectOverlay(RoomEffectBlend blend) => Blend = Ensure.NotNull(blend);

    /// <summary>
    /// How the overlay blends with the room (#1545): lying over it (<see cref="OverlayOverRoomBlend"/>) or behind it
    /// (<see cref="OverlayBehindRoomBlend"/>).
    /// </summary>
    public RoomEffectBlend Blend { get; }
}
