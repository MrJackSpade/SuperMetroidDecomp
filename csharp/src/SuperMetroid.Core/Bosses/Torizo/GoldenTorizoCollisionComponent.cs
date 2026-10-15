namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Physical component of one Golden Torizo extended frame; not editable art.</summary>
/// <param name="Offset">Component position relative to Torizo's room origin, reflected for mirrored right-facing frames.</param>
/// <param name="HitboxList">Bank-$AA pointer to ordered touch and shot rectangles, or the shared empty list.</param>
internal readonly record struct GoldenTorizoCollisionComponent(
    Offset16<RoomPixels> Offset, ushort HitboxList);
