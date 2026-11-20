using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A room effect lying behind the room (#1514, #1545), as the spores and the waterfalls do: the effect's overlay, and the
/// background where the effect lays it behind too, change the layers the effect names where each is frontmost, whole,
/// everywhere on the screen.
/// </summary>
/// <param name="Effect">How the room's effect, lying behind the room, blends.</param>
public sealed record EffectBehindRoomBlend(OverlayBehindRoomBlend Effect) : GameplayBlend;
