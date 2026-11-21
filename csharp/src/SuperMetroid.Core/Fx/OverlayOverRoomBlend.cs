namespace SuperMetroid.Core.Fx;

/// <summary>
/// A room effect's overlay lying over the room (#1545, #2505), as the liquids', the fog's and the rain's do: it brightens
/// or darkens whatever the frame shows under it.
/// </summary>
public sealed partial record OverlayOverRoomBlend : RoomEffectBlend;
