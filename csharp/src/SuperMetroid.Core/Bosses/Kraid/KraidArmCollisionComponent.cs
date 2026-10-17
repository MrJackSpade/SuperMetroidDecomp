namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One physical arm component of an extended spritemap.</summary>
/// <param name="Offset">The component origin relative to Kraid's arm enemy, in room pixels.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; the first the probe overlaps wins.</param>
internal readonly record struct KraidArmCollisionComponent(Offset16<RoomPixels> Offset, KraidArmHitboxSequence Hitboxes);
