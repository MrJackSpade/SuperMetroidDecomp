namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of an arm frame's collision components as authored: where it sits and the rectangles it collides with.</summary>
/// <param name="Offset">The component's origin relative to Kraid's arm, in room pixels.</param>
/// <param name="Hitboxes">The hitbox list the component collides with.</param>
internal readonly record struct KraidArmComponentDefinition(Offset16<RoomPixels> Offset, KraidArmHitboxList Hitboxes);
