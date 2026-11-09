using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What a Draygon wall cannon of one facing shows, and for how long.</summary>
/// <param name="Shield">The shield a whole cannon shows while it rests.</param>
/// <param name="HitFlash">The shield's flash after a hit that does not destroy the cannon, in order; the shield follows.</param>
/// <param name="Smoke">The destroyed cannon's smoke, in order, looping for good.</param>
internal sealed record DraygonCannonLook(
    TerrainChangeFrame Shield,
    IReadOnlyList<TerrainChangeFrame> HitFlash,
    IReadOnlyList<TerrainChangeFrame> Smoke);
