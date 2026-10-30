using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One wall cannon paired with its projectile origin.</summary>
internal readonly record struct DraygonCannonTarget(
    DraygonWallCannon Cannon,
    Position16<RoomPixels> Origin);
